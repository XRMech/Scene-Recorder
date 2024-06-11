const express = require('express'); // Import express for handling HTTP requests
const multer = require('multer'); // Import multer for handling file uploads
const { RTCPeerConnection, RTCSessionDescription } = require('wrtc'); // Import WebRTC classes for handling video streams
const mongoose = require('mongoose'); // Import mongoose for interacting with MongoDB
const fs = require('fs'); // Import filesystem module for handling file operations
const path = require('path'); // Import path module for handling file paths
const { spawn } = require('child_process'); // Import child_process module for spawning subprocesses

const app = express(); // Create an Express application
const port = 3000; // Define the port the server will run on

// MongoDB setup
mongoose.connect('mongodb://mongo:27017/imageDB', { useNewUrlParser: true, useUnifiedTopology: true }); // Connect to MongoDB

// Define a schema for storing image data in MongoDB
const imageSchema = new mongoose.Schema({
  filename: String,
  timestamp: { type: Date, default: Date.now },
  event: String
});

const Image = mongoose.model('Image', imageSchema); // Create a model based on the schema

app.use(express.json()); // Middleware to parse JSON request bodies

// Serve static files from the 'public' directory
app.use(express.static('public'));

// Set up storage for image uploads in memory
const storage = multer.memoryStorage();
const upload = multer({ storage: storage });

// Endpoint for image upload
app.post('/upload-image', upload.single('image'), (req, res) => {
  const imgBuffer = req.file.buffer; // Get the uploaded image buffer
  const filename = `frame-${Date.now()}.jpg`; // Generate a unique filename based on the current timestamp

  // Save the image buffer to the uploads directory
  fs.writeFile(path.join(__dirname, 'uploads', filename), imgBuffer, (err) => {
    if (err) {
      console.error('Error saving file:', err);
      res.status(500).send('Server Error');
      return;
    }

    // Create a new image document and save it to MongoDB
    const newImage = new Image({
      filename: filename,
      event: req.body.event
    });

    newImage.save((err) => {
      if (err) {
        console.error('Error saving to database:', err);
        res.status(500).send('Database Error');
        return;
      }

      res.status(200).send('File uploaded and saved');
    });
  });
});

// Endpoint to clear all data and images
app.post('/clear-data', async (req, res) => {
  try {
    // Clear all documents in the image collection
    await Image.deleteMany({});

    // Remove all files in the uploads directory
    const uploadsDir = path.join(__dirname, 'uploads');
    fs.readdir(uploadsDir, (err, files) => {
      if (err) throw err;

      for (const file of files) {
        fs.unlink(path.join(uploadsDir, file), err => {
          if (err) throw err;
        });
      }
    });

    res.status(200).send('All data and images cleared');
  } catch (error) {
    console.error('Error clearing data:', error);
    res.status(500).send('Error clearing data');
  }
});

// WebRTC setup for video streaming
let pc = new RTCPeerConnection({
  iceServers: [{ urls: 'stun:stun.l.google.com:19302' }] // Use Google's STUN server for NAT traversal
});

pc.ontrack = (event) => {
  const videoStream = event.streams[0]; // Get the first media stream from the event
  const filename = `output-${Date.now()}.mp4`; // Generate a unique filename for the output video
  const filePath = path.join(__dirname, 'uploads', filename); // Get the full file path for the output video

  console.log(`Received video stream. Saving to ${filePath}`);

  // Spawn an ffmpeg process to save the video stream to a file
  const ffmpeg = spawn('ffmpeg', [
    '-i', 'pipe:0', // Read input from a pipe (stdin)
    '-c:v', 'libx264', // Use the x264 codec for video encoding
    '-preset', 'fast', // Use the 'fast' preset for encoding
    '-crf', '22', // Set the constant rate factor (quality)
    '-y', // Overwrite the output file if it exists
    filePath // Output file path
  ]);

  ffmpeg.stdin.on('error', (e) => console.error('ffmpeg stdin error:', e));
  ffmpeg.stderr.on('data', (data) => console.error('ffmpeg stderr:', data.toString()));
  ffmpeg.on('close', (code) => console.log(`ffmpeg process exited with code ${code}`));

  const mediaRecorder = new MediaRecorder(videoStream);

  mediaRecorder.ondataavailable = (event) => {
    if (event.data.size > 0) {
      console.log('Writing data to ffmpeg');
      ffmpeg.stdin.write(event.data); // Write the recorded data to ffmpeg's stdin
    }
  };

  mediaRecorder.onstop = () => {
    console.log('MediaRecorder stopped');
    ffmpeg.stdin.end(); // Close ffmpeg's stdin to finalize the file
  };

  mediaRecorder.onerror = (error) => {
    console.error('MediaRecorder error:', error);
  };

  mediaRecorder.start(1000); // Start recording in 1-second intervals
  console.log('MediaRecorder started');
};

// Endpoint to handle SDP offers from the client
app.post('/offer', async (req, res) => {
  try {
    const offer = req.body; // Get the offer from the request body
    console.log('Received offer:', offer);

    if (!offer || offer.type !== 'offer' || !offer.sdp) {
      console.error('Invalid SDP offer received:', offer);
      return res.status(400).send('Invalid SDP offer');
    }

    if (pc.signalingState !== 'stable') {
      console.error('Signaling state is not stable. Cannot process new offer.');
      return res.status(400).send('Signaling state is not stable');
    }

    console.log('Setting remote description...');
    await pc.setRemoteDescription(new RTCSessionDescription(offer));
    console.log('Remote description set. Creating answer...');
    const answer = await pc.createAnswer();
    console.log('Answer created. Setting local description...');
    await pc.setLocalDescription(answer);
    console.log('Local description set. Sending answer back to client...');
    res.json(pc.localDescription);
    console.log('Sent answer:', pc.localDescription);
  } catch (error) {
    console.error('Error handling SDP offer:', error);
    res.status(500).send('Internal Server Error');
  }
});

// Event handler for ICE candidates
pc.onicecandidate = (event) => {
  if (event.candidate) {
    console.log('ICE candidate:', event.candidate);
    // Handle ICE candidate (this can be expanded to send the candidate to the peer)
  }
};

// Endpoint to list recorded videos
app.get('/videos', (req, res) => {
  const uploadsDir = path.join(__dirname, 'uploads'); // Get the uploads directory path
  fs.readdir(uploadsDir, (err, files) => {
    if (err) {
      console.error('Error reading uploads directory:', err);
      res.status(500).send('Server Error');
      return;
    }

    const videoFiles = files.filter(file => file.endsWith('.mp4')); // Filter for .mp4 files
    res.json(videoFiles); // Send the list of video files as a JSON response
  });
});

// Endpoint to serve video files
app.get('/videos/:filename', (req, res) => {
  const filename = req.params.filename; // Get the filename from the request parameters
  const filePath = path.join(__dirname, 'uploads', filename); // Get the full file path for the video file

  fs.access(filePath, fs.constants.F_OK, (err) => {
    if (err) {
      res.status(404).send('File not found'); // Send a 404 response if the file doesn't exist
      return;
    }

    res.sendFile(filePath); // Send the video file as a response
  });
});

// Start the Express server
app.listen(port, () => {
  console.log(`Server running at http://localhost:${port}/`);
});
