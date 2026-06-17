/**
 * FLUTTER PHOTO UPLOAD INTEGRATION GUIDE
 * 
 * This document provides Flutter developers with examples of how to upload photos
 * to the TourismG API from mobile devices.
 */

// ============================================================================
// 1. DEPENDENCIES (pubspec.yaml)
// ============================================================================
/*
dependencies:
  flutter:
	sdk: flutter
  dio: ^5.3.0
  image_picker: ^1.0.0
  flutter_dotenv: ^5.1.0

dev_dependencies:
  flutter_test:
	sdk: flutter
*/

// ============================================================================
// 2. BASIC SETUP - Create API Client
// ============================================================================
/*
import 'package:dio/dio.dart';

class ApiClient {
  late Dio _dio;
  final String _baseUrl = "https://your-api-domain.com"; // Replace with your API URL

  ApiClient() {
	_dio = Dio(
	  BaseOptions(
		baseUrl: _baseUrl,
		connectTimeout: Duration(seconds: 30),
		receiveTimeout: Duration(seconds: 30),
	  ),
	);

	// Add token interceptor for authenticated requests
	_dio.interceptors.add(
	  InterceptorsWrapper(
		onRequest: (options, handler) {
		  // Add JWT token if available
		  final token = getStoredToken(); // Your token retrieval logic
		  if (token != null) {
			options.headers['Authorization'] = 'Bearer $token';
		  }
		  return handler.next(options);
		},
	  ),
	);
  }

  Dio get dio => _dio;
}
*/

// ============================================================================
// 3. SERVICE PHOTO UPLOAD (Provider Services)
// ============================================================================
/*
import 'package:image_picker/image_picker.dart';
import 'dio:dio.dart';

Future<void> uploadServicePhoto(
  Dio dio,
  String serviceId,
  XFile imageFile,
) async {
  try {
	// Create FormData with file
	FormData formData = FormData.fromMap({
	  'photo': await MultipartFile.fromFile(
		imageFile.path,
		filename: imageFile.name,
	  ),
	});

	// POST request
	Response response = await dio.post(
	  '/api/provider/services/$serviceId/upload-photo',
	  data: formData,
	  options: Options(
		contentType: 'multipart/form-data',
	  ),
	);

	if (response.statusCode == 200) {
	  final data = response.data;
	  if (data['success']) {
		final fileUrl = data['fileUrl'];
		print('Photo uploaded successfully: $fileUrl');
		// Update UI with the new image URL
	  } else {
		print('Upload failed: ${data['errorMessage']}');
	  }
	}
  } on DioException catch (e) {
	print('Error uploading service photo: $e');
	// Handle specific error cases
	if (e.response?.statusCode == 401) {
	  print('Unauthorized - re-authenticate required');
	} else if (e.response?.statusCode == 400) {
	  print('Invalid file - check file type and size');
	}
  }
}

// Usage example:
/*
final imageFile = await ImagePicker().pickImage(source: ImageSource.gallery);
if (imageFile != null) {
  await uploadServicePhoto(apiClient.dio, serviceId, imageFile);
}
*/
*/

// ============================================================================
// 4. PLACE PHOTO UPLOAD
// ============================================================================
/*
Future<void> uploadPlacePhoto(
  Dio dio,
  String placeId,
  XFile imageFile,
) async {
  try {
	FormData formData = FormData.fromMap({
	  'photo': await MultipartFile.fromFile(
		imageFile.path,
		filename: imageFile.name,
	  ),
	});

	Response response = await dio.post(
	  '/api/provider/places/$placeId/upload-photo',
	  data: formData,
	);

	if (response.statusCode == 200) {
	  final fileUrl = response.data['fileUrl'];
	  print('Place photo uploaded: $fileUrl');
	}
  } catch (e) {
	print('Error uploading place photo: $e');
  }
}
*/

// ============================================================================
// 5. USER AVATAR UPLOAD (Profile)
// ============================================================================
/*
Future<void> uploadUserAvatar(
  Dio dio,
  XFile avatarFile,
) async {
  try {
	FormData formData = FormData.fromMap({
	  'photo': await MultipartFile.fromFile(
		avatarFile.path,
		filename: avatarFile.name,
	  ),
	});

	Response response = await dio.post(
	  '/api/profile/upload-avatar',
	  data: formData,
	);

	if (response.statusCode == 200) {
	  final profileImageUrl = response.data['fileUrl'];
	  print('Avatar updated: $profileImageUrl');
	  // Update local state/storage with new avatar URL
	}
  } catch (e) {
	print('Error uploading avatar: $e');
  }
}
*/

// ============================================================================
// 6. REVIEW PHOTO UPLOAD
// ============================================================================
/*
Future<void> uploadReviewPhoto(
  Dio dio,
  String reviewId,
  XFile photoFile,
) async {
  try {
	FormData formData = FormData.fromMap({
	  'photo': await MultipartFile.fromFile(
		photoFile.path,
		filename: photoFile.name,
	  ),
	});

	Response response = await dio.post(
	  '/api/reviews/$reviewId/upload-photo',
	  data: formData,
	);

	if (response.statusCode == 200) {
	  final photoUrl = response.data['fileUrl'];
	  print('Review photo uploaded: $photoUrl');
	}
  } catch (e) {
	print('Error uploading review photo: $e');
  }
}
*/

// ============================================================================
// 7. COMPLETE EXAMPLE WIDGET - Image Picker with Upload
// ============================================================================
/*
import 'package:flutter/material.dart';
import 'package:image_picker/image_picker.dart';

class PhotoUploadWidget extends StatefulWidget {
  final String entityId; // Service ID, Place ID, or Review ID
  final String uploadType; // 'service', 'place', 'avatar', or 'review'
  final Dio dio;

  const PhotoUploadWidget({
	required this.entityId,
	required this.uploadType,
	required this.dio,
  });

  @override
  State<PhotoUploadWidget> createState() => _PhotoUploadWidgetState();
}

class _PhotoUploadWidgetState extends State<PhotoUploadWidget> {
  final ImagePicker _imagePicker = ImagePicker();
  bool _isUploading = false;
  String? _uploadError;

  Future<void> _pickAndUpload() async {
	try {
	  final XFile? image = await _imagePicker.pickImage(
		source: ImageSource.gallery,
		imageQuality: 80, // Compress image
	  );

	  if (image == null) return;

	  setState(() => _isUploading = true);

	  String endpoint = _getEndpoint();

	  FormData formData = FormData.fromMap({
		'photo': await MultipartFile.fromFile(image.path),
	  });

	  Response response = await widget.dio.post(
		endpoint,
		data: formData,
	  );

	  if (response.statusCode == 200 && response.data['success']) {
		setState(() {
		  _uploadError = null;
		  _isUploading = false;
		});

		// Show success message
		ScaffoldMessenger.of(context).showSnackBar(
		  const SnackBar(content: Text('Photo uploaded successfully!')),
		);
	  } else {
		throw Exception('Upload failed: ${response.data['errorMessage']}');
	  }
	} catch (e) {
	  setState(() {
		_uploadError = e.toString();
		_isUploading = false;
	  });

	  ScaffoldMessenger.of(context).showSnackBar(
		SnackBar(content: Text('Error: $e')),
	  );
	}
  }

  String _getEndpoint() {
	switch (widget.uploadType) {
	  case 'service':
		return '/api/provider/services/${widget.entityId}/upload-photo';
	  case 'place':
		return '/api/provider/places/${widget.entityId}/upload-photo';
	  case 'avatar':
		return '/api/profile/upload-avatar';
	  case 'review':
		return '/api/reviews/${widget.entityId}/upload-photo';
	  default:
		throw Exception('Unknown upload type');
	}
  }

  @override
  Widget build(BuildContext context) {
	return Column(
	  children: [
		ElevatedButton.icon(
		  onPressed: _isUploading ? null : _pickAndUpload,
		  icon: _isUploading
			  ? SizedBox(
				  width: 20,
				  height: 20,
				  child: CircularProgressIndicator(strokeWidth: 2),
				)
			  : Icon(Icons.image),
		  label: Text(_isUploading ? 'Uploading...' : 'Pick & Upload Photo'),
		),
		if (_uploadError != null)
		  Padding(
			padding: EdgeInsets.only(top: 8),
			child: Text(
			  'Error: $_uploadError',
			  style: TextStyle(color: Colors.red),
			),
		  ),
	  ],
	);
  }
}
*/

// ============================================================================
// API RESPONSE FORMAT
// ============================================================================
/*
Success Response (200):
{
  "success": true,
  "fileUrl": "/api/files/uploads/20260617_130059_a1b2c3d4.jpg"
}

Error Response (400):
{
  "success": false,
  "fileUrl": null,
  "errorMessage": "Invalid file. Allowed formats: JPG, PNG, WEBP, GIF, BMP. Max size: 5 MB"
}

Note: Use the returned fileUrl to display images in your Flutter app:
Image.network(
  'https://your-api-domain.com$fileUrl',
  // or if using relative URL resolution:
  // 'https://your-api-domain.com${response.data['fileUrl']}',
)
*/

// ============================================================================
// IMPORTANT NOTES FOR FLUTTER DEVELOPERS
// ============================================================================
/*
1. AUTHENTICATION:
   - All upload endpoints require JWT Bearer token
   - Include token in Authorization header: 'Bearer <your_jwt_token>'
   - Token must be valid and user must have appropriate role (Provider, Customer, etc.)

2. FILE REQUIREMENTS:
   - Allowed formats: JPG, PNG, WEBP, GIF, BMP, TIFF, ICO
   - Maximum file size: 5 MB
   - Larger files will be rejected with 400 error

3. IMAGE COMPRESSION:
   - Recommended: Compress images before upload using imageQuality parameter:
	 await ImagePicker().pickImage(
	   source: ImageSource.gallery,
	   imageQuality: 80,  // 0-100, 80 is a good balance
	 );
   - This reduces bandwidth usage and upload time

4. ERROR HANDLING:
   - 401 Unauthorized: User not authenticated or token expired - re-authenticate
   - 400 Bad Request: Invalid file format or size - check file and retry
   - 403 Forbidden: User doesn't have permission for this resource
   - 404 Not Found: Entity (service/place/review/user) not found
   - 500 Server Error: Contact API administrator

5. URL HANDLING:
   - Returned fileUrl is relative: /api/files/uploads/...
   - Use full URL when displaying: https://your-domain.com/api/files/uploads/...
   - Store fileUrl for later retrieval and displaying

6. RETRY LOGIC:
   - Implement exponential backoff for network failures
   - Allow users to re-upload if connection fails
   - Show clear error messages and suggestions

7. BACKGROUND UPLOADS:
   - For better UX, consider using background upload packages
   - Use workmanager or similar for persistent uploads
   - Don't block UI while uploading large files

8. PERFORMANCE:
   - Display upload progress: use onSendProgress in Dio
   - Show loading indicators to users
   - Clean up failed uploads
*/
