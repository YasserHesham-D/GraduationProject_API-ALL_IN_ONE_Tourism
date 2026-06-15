# TourismG API - New Endpoints Documentation

## Base URL
```
https://localhost:5001/api
```

## Authentication
All endpoints except public ones require a Bearer JWT token:
```
Authorization: Bearer {token}
```

---

## Hotels Endpoints

### 1. Get All Hotels
```
GET /hotels
```
**Description**: Retrieve all available hotels
**Auth**: Not required
**Response**: 
```json
{
  "success": true,
  "data": [
	{
	  "id": "guid",
	  "name": "Hotel Name",
	  "location": "Cairo",
	  "city": "Cairo",
	  "country": "Egypt",
	  "description": "Hotel description",
	  "imageUrl": "https://...",
	  "starRating": 5,
	  "pricePerNight": 150.00,
	  "rating": 4.5,
	  "reviewCount": 25,
	  "availableRooms": 10,
	  "amenities": "WiFi, Pool, Restaurant",
	  "contactNumber": "+20123456789",
	  "email": "hotel@example.com"
	}
  ]
}
```

### 2. Get Hotel by ID
```
GET /hotels/{id}
```
**Description**: Get details of a specific hotel
**Auth**: Not required
**Parameters**: `id` (Guid)
**Response**: Single hotel object

### 3. Book a Hotel
```
POST /hotels/{id}/book
```
**Description**: Book a hotel
**Auth**: Required (JWT token)
**Parameters**: `id` (Guid)
**Request Body**:
```json
{
  "checkInDate": "2025-01-15T00:00:00",
  "checkOutDate": "2025-01-20T00:00:00",
  "numberOfRooms": 2,
  "numberOfGuests": 4,
  "specialRequests": "High floor preferred"
}
```
**Response**:
```json
{
  "success": true,
  "data": {
	"id": "guid",
	"hotelId": "guid",
	"hotelName": "Hotel Name",
	"checkInDate": "2025-01-15T00:00:00",
	"checkOutDate": "2025-01-20T00:00:00",
	"numberOfRooms": 2,
	"numberOfGuests": 4,
	"totalPrice": 1500.00,
	"status": "pending"
  },
  "message": "Hotel booked successfully"
}
```

---

## Transport Endpoints

### 1. Get All Transport Options
```
GET /transport
```
**Description**: Retrieve all available transportation options
**Auth**: Not required

### 2. Get Transport by ID
```
GET /transport/{id}
```
**Description**: Get details of a specific transport option
**Auth**: Not required
**Parameters**: `id` (Guid)

### 3. Book Transport
```
POST /transport/{id}/book
```
**Description**: Book a transport service
**Auth**: Required (JWT token)
**Parameters**: `id` (Guid)
**Request Body**:
```json
{
  "numberOfSeats": 3
}
```
**Response**:
```json
{
  "success": true,
  "data": {
	"id": "guid",
	"transportId": "guid",
	"transportName": "Bus Name",
	"numberOfSeats": 3,
	"totalPrice": 450.00,
	"status": "pending",
	"bookingDate": "2025-01-10T10:00:00"
  },
  "message": "Transport booked successfully"
}
```

---

## Programs Endpoints

### 1. Get All Programs
```
GET /programs
```
**Description**: Retrieve all available tour programs
**Auth**: Not required

### 2. Get Program by ID
```
GET /programs/{id}
```
**Description**: Get details of a specific program
**Auth**: Not required
**Parameters**: `id` (Guid)

### 3. Book a Program
```
POST /programs/{id}/book
```
**Description**: Book a tour program
**Auth**: Required (JWT token)
**Parameters**: `id` (Guid)
**Request Body**:
```json
{
  "numberOfParticipants": 4
}
```
**Response**:
```json
{
  "success": true,
  "data": {
	"id": "guid",
	"programId": "guid",
	"programName": "Desert Safari",
	"numberOfParticipants": 4,
	"totalPrice": 800.00,
	"status": "pending",
	"bookingDate": "2025-01-10T10:00:00"
  },
  "message": "Program booked successfully"
}
```

---

## Guides Endpoints

### 1. Get All Guides
```
GET /guides
```
**Description**: Retrieve all available tour guides
**Auth**: Not required

### 2. Get Guide by ID
```
GET /guides/{id}
```
**Description**: Get details of a specific guide
**Auth**: Not required
**Parameters**: `id` (Guid)

### 3. Book a Guide
```
POST /guides/{id}/book
```
**Description**: Book a tour guide
**Auth**: Required (JWT token)
**Parameters**: `id` (Guid)
**Request Body**:
```json
{
  "startDate": "2025-01-15T08:00:00",
  "endDate": "2025-01-20T18:00:00",
  "numberOfPeople": 4,
  "specialRequests": "English speaker preferred"
}
```
**Response**:
```json
{
  "success": true,
  "data": {
	"id": "guid",
	"guideId": "guid",
	"guideName": "Ahmed Mohamed",
	"startDate": "2025-01-15T08:00:00",
	"endDate": "2025-01-20T18:00:00",
	"numberOfPeople": 4,
	"numberOfDays": 5,
	"totalPrice": 1000.00,
	"status": "pending"
  },
  "message": "Guide booked successfully"
}
```

---

## Provider Endpoints

### 1. Submit Provider Request
```
POST /provider/request
```
**Description**: Submit a request to become a provider
**Auth**: Required (JWT token)
**Request Body**:
```json
{
  "businessName": "My Tourism Business",
  "businessType": "Hotel",
  "businessDescription": "Luxury hotel chain",
  "contactNumber": "+20123456789",
  "email": "business@example.com",
  "taxNumber": "TAX123456",
  "registrationNumber": "REG123456",
  "documentUrl": "https://..."
}
```
**Response**:
```json
{
  "success": true,
  "data": {
	"id": "guid",
	"businessName": "My Tourism Business",
	"businessType": "Hotel",
	"status": "pending",
	"submittedAt": "2025-01-10T10:00:00",
	"reviewedAt": null,
	"rejectionReason": null
  },
  "message": "Provider request submitted successfully"
}
```

### 2. Get My Provider Request
```
GET /provider/request/my
```
**Description**: Get your current provider request status
**Auth**: Required (JWT token)
**Response**: Provider request object

### 3. Get Provider Earnings
```
GET /provider/earnings
```
**Description**: Get your earnings summary
**Auth**: Required (JWT token)
**Response**:
```json
{
  "success": true,
  "data": {
	"id": "guid",
	"totalEarnings": 5000.00,
	"pendingEarnings": 1000.00,
	"withdrawnAmount": 4000.00,
	"completedBookings": 25,
	"lastUpdated": "2025-01-10T10:00:00"
  }
}
```

### 4. Confirm Booking
```
PUT /provider/bookings/{id}/confirm
```
**Description**: Confirm a booking
**Auth**: Required (JWT token - Provider)
**Parameters**: `id` (Guid)
**Response**:
```json
{
  "success": true,
  "message": "Booking confirmed successfully"
}
```

### 5. Decline Booking
```
PUT /provider/bookings/{id}/decline
```
**Description**: Decline a booking
**Auth**: Required (JWT token - Provider)
**Parameters**: `id` (Guid)
**Response**:
```json
{
  "success": true,
  "message": "Booking declined successfully"
}
```

### 6. Complete Booking
```
PUT /provider/bookings/{id}/complete
```
**Description**: Mark a booking as completed
**Auth**: Required (JWT token - Provider)
**Parameters**: `id` (Guid)
**Response**:
```json
{
  "success": true,
  "message": "Booking completed successfully"
}
```

### 7. Contact User
```
POST /provider/bookings/{id}/contact
```
**Description**: Send a message to the user regarding a booking
**Auth**: Required (JWT token - Provider)
**Parameters**: `id` (Guid)
**Request Body**:
```json
{
  "message": "Your booking has been confirmed for January 15th"
}
```
**Response**:
```json
{
  "success": true,
  "message": "Message sent to user successfully"
}
```

---

## Admin Endpoints

### 1. Get All Pending Provider Requests
```
GET /admin/provider-requests
```
**Description**: Get all pending provider registration requests
**Auth**: Required (JWT token - Admin role)
**Response**:
```json
{
  "success": true,
  "data": [
	{
	  "id": "guid",
	  "businessName": "My Business",
	  "businessType": "Hotel",
	  "status": "pending",
	  "submittedAt": "2025-01-10T10:00:00",
	  "reviewedAt": null,
	  "rejectionReason": null
	}
  ]
}
```

### 2. Get Provider Request by ID
```
GET /admin/provider-requests/{id}
```
**Description**: Get a specific provider request
**Auth**: Required (JWT token - Admin role)
**Parameters**: `id` (Guid)
**Response**: Provider request object

### 3. Approve Provider Request
```
PUT /admin/provider-requests/{id}/approve
```
**Description**: Approve a provider request
**Auth**: Required (JWT token - Admin role)
**Parameters**: `id` (Guid)
**Response**:
```json
{
  "success": true,
  "data": {
	"id": "guid",
	"businessName": "My Business",
	"status": "approved"
  },
  "message": "Provider request approved successfully"
}
```

### 4. Reject Provider Request
```
PUT /admin/provider-requests/{id}/reject
```
**Description**: Reject a provider request
**Auth**: Required (JWT token - Admin role)
**Parameters**: `id` (Guid)
**Request Body**:
```json
{
  "rejectionReason": "Documentation is incomplete"
}
```
**Response**:
```json
{
  "success": true,
  "data": {
	"id": "guid",
	"businessName": "My Business",
	"status": "rejected",
	"rejectionReason": "Documentation is incomplete"
  },
  "message": "Provider request rejected successfully"
}
```

---

## Testing with Postman/Insomnia

### Test Steps:

1. **Get Hotels** (No auth needed):
   - GET `https://localhost:5001/api/hotels`

2. **Book Hotel** (Auth needed):
   - POST `https://localhost:5001/api/hotels/{hotel-id}/book`
   - Add header: `Authorization: Bearer {your-jwt-token}`
   - Body: Check example above

3. **Submit Provider Request**:
   - POST `https://localhost:5001/api/provider/request`
   - Auth required

4. **Admin Approval** (Admin role required):
   - PUT `https://localhost:5001/api/admin/provider-requests/{request-id}/approve`
   - Auth required with Admin role

---

## Error Responses

### 400 Bad Request
```json
{
  "success": false,
  "message": "Invalid input or business logic error"
}
```

### 401 Unauthorized
```json
{
  "success": false,
  "message": "User not authenticated"
}
```

### 403 Forbidden
```json
{
  "success": false,
  "message": "User lacks required permissions"
}
```

### 404 Not Found
```json
{
  "success": false,
  "message": "Resource not found"
}
```

### 500 Internal Server Error
```json
{
  "success": false,
  "message": "Internal server error"
}
```
