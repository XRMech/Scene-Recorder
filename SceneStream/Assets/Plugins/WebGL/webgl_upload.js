// mergeInto(LibraryManager.library, {
//     UploadImageToS3: function(imageData, length, fileNamePtr) {
//         var bytes = new Uint8Array(Module.HEAPU8.buffer, imageData, length);
//         var blob = new Blob([bytes], { type: 'image/jpeg' });
//
//         var fileName = UTF8ToString(fileNamePtr);
//
//         AWS.config.update({
//             accessKeyId: 'YOUR_AWS_ACCESS_KEY_ID',
//             secretAccessKey: 'YOUR_AWS_SECRET_ACCESS_KEY',
//             region: 'YOUR_AWS_REGION'
//         });
//
//         var s3 = new AWS.S3();
//         var params = {
//             Bucket: 'YOUR_S3_BUCKET_NAME',
//             Key: fileName,
//             Body: blob,
//             ContentType: 'image/jpeg'
//         };
//
//         s3.upload(params, function(err, data) {
//             if (err) {
//                 console.error('Error uploading image to S3:', err);
//             } else {
//                 console.log('Successfully uploaded image to S3:', data);
//             }
//         });
//     }
// });
