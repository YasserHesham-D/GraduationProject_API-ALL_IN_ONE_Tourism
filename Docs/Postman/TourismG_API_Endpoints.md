# TourismG API Postman Documentation

Import `TourismG_API.postman_collection.json` into Postman. Set `baseUrl` to the running API URL, for example `https://localhost:5001`.

Most protected endpoints use bearer auth. Run `Accounts / Sign In` first; the collection stores the response `Token` into `{{token}}`.

## Variables

| Variable | Purpose |
| --- | --- |
| `baseUrl` | API host URL |
| `token` | JWT bearer token |
| `placeId` | Destination/place id |
| `serviceId` | Service offering id |
| `tripId` | Trip id |
| `dayId` | Trip day id |
| `activityId` | Trip activity id |
| `bookingId` | Booking id |
| `resetToken` | Password reset token from forgot password |
| `category` | Place category, for example `Historical` |

## Accounts

| Method | Endpoint | Auth | Body |
| --- | --- | --- | --- |
| POST | `/api/Accounts/SignUp` | No | `fullName`, `email`, `password`, `confirmPassword`, `address`, `role` |
| POST | `/api/Accounts/SignIn` | No | `email`, `password` |
| GET | `/api/Accounts/TestAuth` | Yes | None |
| POST | `/api/Accounts/Logout` | Yes | None |
| GET | `/api/Accounts/GetUsername` | Yes | None |
| POST | `/api/Accounts/ForgotPassword` | No | `email` |
| POST | `/api/Accounts/ResetPassword` | No | `email`, `resetToken`, `newPassword`, `confirmPassword` |
| POST | `/api/Accounts/ChangePassword` | Yes | `currentPassword`, `newPassword`, `confirmPassword` |

## Profile

| Method | Endpoint | Auth | Body |
| --- | --- | --- | --- |
| GET | `/api/Profile/me` | Yes | None |
| PUT | `/api/Profile/me` | Yes | `fullName`, `address` |

## Places

| Method | Endpoint | Auth | Query/Body |
| --- | --- | --- | --- |
| GET | `/api/Places` | No | `search`, `category`, `recommended`, `popular`, `page`, `pageSize` |
| GET | `/api/Places/{placeId}` | No | None |
| GET | `/api/Places/category/{category}` | No | `page`, `pageSize` |
| GET | `/api/Places/summary` | No | None |
| GET | `/api/Places/{placeId}/nearby` | No | `take` |
| GET | `/api/Places/categories` | No | None |

## Search

| Method | Endpoint | Auth | Query |
| --- | --- | --- | --- |
| GET | `/api/Search` | No | `query`, `take` |

## Saved And Visited Places

| Method | Endpoint | Auth | Body |
| --- | --- | --- | --- |
| GET | `/api/user/places/saved` | Yes | None |
| POST | `/api/user/places/saved/{placeId}` | Yes | None |
| DELETE | `/api/user/places/saved/{placeId}` | Yes | None |
| GET | `/api/user/places/visited` | Yes | None |
| POST | `/api/user/places/visited/{placeId}` | Yes | None |

## Reviews

| Method | Endpoint | Auth | Body |
| --- | --- | --- | --- |
| GET | `/api/Reviews/places/{placeId}` | No | None |
| POST | `/api/Reviews/places/{placeId}` | Yes | `rating`, `comment` |
| GET | `/api/Reviews/trips/{tripId}` | No | None |
| POST | `/api/Reviews/trips/{tripId}` | Yes | `rating`, `comment` |

## Services

| Method | Endpoint | Auth | Query/Body |
| --- | --- | --- | --- |
| GET | `/api/Services` | No | `category`, `search`, `page`, `pageSize` |
| GET | `/api/Services/{serviceId}` | No | None |
| POST | `/api/Services/{serviceId}/book` | Yes | `bookingDate`, `guests` |

## Bookings

| Method | Endpoint | Auth | Body |
| --- | --- | --- | --- |
| GET | `/api/Bookings/my` | Yes | None |
| DELETE | `/api/Bookings/{bookingId}` | Yes | None |

## Trips

| Method | Endpoint | Auth | Body |
| --- | --- | --- | --- |
| GET | `/api/Trips` | Yes | None |
| GET | `/api/Trips/{tripId}` | Yes | None |
| POST | `/api/Trips` | Yes | `title`, `startDate`, `endDate`, `notes` |
| PUT | `/api/Trips/{tripId}` | Yes | `title`, `notes` |
| DELETE | `/api/Trips/{tripId}` | Yes | None |
| POST | `/api/Trips/{tripId}/days/{dayId}/activities` | Yes | `title`, `scheduledAt`, `placeId`, `serviceOfferingId`, `notes` |
| DELETE | `/api/Trips/{tripId}/activities/{activityId}` | Yes | None |

## Provider

| Method | Endpoint | Auth | Body |
| --- | --- | --- | --- |
| GET | `/api/provider/dashboard` | Yes | None |
| GET | `/api/provider/services` | Yes | None |
| GET | `/api/provider/services/{serviceId}` | Yes | None |
| POST | `/api/provider/services` | Yes | `placeId`, `title`, `category`, `description`, `price`, `currency`, `duration`, `locationName`, `imageUrl`, `availability`, `rating`, `isActive` |
| PUT | `/api/provider/services/{serviceId}` | Yes | Same as add service |
| DELETE | `/api/provider/services/{serviceId}` | Yes | None |
| GET | `/api/provider/bookings` | Yes | None |
| PUT | `/api/provider/bookings/{bookingId}/status` | Yes | `status` |
