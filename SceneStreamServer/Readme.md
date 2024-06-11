
# Scene Stream Game Streaming and Video Recording
This is a work in progress and may not function fully yet. 

This repository contains a Node.js application for uploading images and performing video streaming. This guide will help you set up the project locally on your machine.

## Prerequisites

Before you begin, ensure you have met the following requirements:

- [Node.js](https://nodejs.org/) (including npm)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Git](https://git-scm.com/)

## Setup Instructions

### 1. Clone the Repository

First, clone the repository to your local machine using Git.


git clone https://github.com/your-username/your-repo.git
cd your-repo

## 2. Install Node.js and npm
If you haven't installed Node.js and npm, download and install them from here. Verify the installation by running:


node --version
npm --version
## 3. Install Dependencies
Navigate to the project directory and install the required Node.js packages:


cd path/to/your/project
npm install
## 4. Set Up MongoDB with Docker
Run MongoDB using Docker. Ensure Docker Desktop is installed and running, then execute the following command to start a MongoDB container:


docker run -d -p 27017:27017 --name mongodb mongo:latest

Verify the MongoDB container is running:


docker ps
## 5. Create Uploads Directory
Ensure the uploads directory exists in your project root. Create it if it does not exist:


mkdir uploads

## 6.Configure Environment Variables (Optional)
If your application requires environment variables, create a .env file in the project root and add your variables. Example:


MONGODB_URI=mongodb://localhost:27017/imageDB
PORT=3000
## 7. Start the Application
Run the Node.js application:


node server.js
The server should now be running at http://localhost:3000.

## 8. Testing the Application
You can use tools like Postman or curl to test the endpoints.

### Upload an Image
Using curl:


curl -X POST http://localhost:3000/upload-image -H "Content-Type: multipart/form-data" -F "image=@path/to/your/image.jpg" -F "event=Sample Event"
Using Postman:

### Create a new POST request.
Set the URL to http://localhost:3000/upload-image.
In the Body section, choose form-data and add the fields:
Key: image, Type: File, Value: (select an image file)
Key: event, Type: Text, Value: Sample Event
Click Send.
Clear Data
Using curl:


curl -X POST http://localhost:3000/clear-data
Using Postman:

Create a new POST request.
Set the URL to http://localhost:3000/clear-data.
Click Send.
## Contributing
If you want to contribute to this project, follow these steps:

Fork the repository.
Create a new branch (git checkout -b feature-branch).
Make your changes and commit them (git commit -m 'Add new feature').
Push to the branch (git push origin feature-branch).
Create a new Pull Request.
License
This project is licensed under the MIT License - see the LICENSE file for details.