# api-reac-android

This API will be used as a middleware between the Android Application and the Database/Logic of the server.
The API will be RESTful and their CRUD operations are defined below:

## General Documentation   

### [IPAddress](#ip_address_extra) 
| Method | Url | Action |
| ------ | ------------------- | --- |
| GET    | /api/ipaddress      | Returns the External IP Address of the Server |


### [Register Administrator](#register_administrator_extra) 
| Method | Url | Action |
| ------ | ------------------- | --- |
| POST    | /api/register      | Register a new Administrator when there isn't any (on first setup) |


### [Login Administrator](#login_administrator_extra) 
| Method | Url | Action |
| ------ | ------------------- | --- |
| GET    | /api/login          | Login for Administrator users |


### [User To Admin](#user_to_admin)
| Method | Url | Action |
| ------ | ------------------- | --- |
| POST   | /api/admin          | Returns a new session_id that will expire in 5 minutes and is required for registering the new admin |

### [User To Admin Confirm](#user_to_admin_confirm)
| Method | Url | Action |
| ------ | ------------------- | --- |
| GET    | /api/admin/confirm  | Returns a password of the new created admin (it should be done from the other android device) |


### [Create User](#create_user)
| Method | Url | Action |
| ------ | ------------------- | --- |
| POST   | /api/user           | Registers a new user into the database |

### [Start Biometric Data User](#start_bio)
| Method | Url | Action |
| ------ | ------------------- | --- |
| POST   | /api/user/biometric | Sends to locker device that an user should be registered (get face and fingerprint) |

### [User Biometric Images List](#user_images_list)
| Method | Url | Action |
| ------ | ------------------- | --- |
| GET    | /api/user/:userid/images  | Retrieves a list with all the photo URIs from that user's face |

### [User Face Image](#user_image)
| Method | Url | Action |
| ------ | ------------------- | --- |
| GET    | /api/user/:userid/face/:imageid | Get the face image from a user |

### [User Profile Image](#user_profile_image)
| Method | Url | Action |
| ------ | ------------------- | --- |
| GET    | /api/user/:userid/profile/image | Get the profile image from a user |

### [Upload User Profile Image](#upload_user_profile_image)
| Method | Url | Action |
| ------ | ------------------- | --- |
| POST    | /api/user/:userid/profile/image | Upload the profile image from a user |


### [Users List](#users_list)
| Method | Url | Action |
| ------ | ------------------- | --- |
| GET    | /api/users          | Retrieves a list with information about every member |


### [Camera Streaming](#camera_streaming)
| Method | Url | Action |
| ------ | ------------------- | --- |
| GET    | /api/video          | Ask the server to give the current snapshot/image back from the video streaming camera |


### [Open Door](#open_door)
| Method | Url | Action |
| ------ | ------------------- | --- |
| GET    | /api/door           | Ask the server to open the door (for 3 seconds) |


### [Get Log](#get_log)
| Method | Url | Action |
| ------ | ------------------- | --- |
| GET    | /api/logs            | Retrieve logs from the house (given a range of dates) |

### [Get Notification Log](#get_notification_log)
| Method | Url | Action |
| ------ | ------------------- | --- |
| GET    | /api/notifications  | Retrieve new logs for displaying in notifications |

### [Set Notification Log Read](#set_notification_log_read)
| Method | Url | Action |
| ------ | ------------------- | --- |
| PUT    | /api/notification/:notification_id | Mark the notification as read |

<!-- NOT DONE YET
## User
| Method | Url | Action |
| ------ | ------------------- | --- |
| POST   | /api/user               | Registers a new user into the database (an one-time password will be returned with a limited time of 5min for the member to add it to his mobile device) |
| PUT    | /api/user/:username     | Updates information about the user (name, profile photo, role, ... |
| DELETE | /api/user/:username     | Delete the instance of user |
-->


## General Documentation all in one table

| Method | Url | Action |
| ------ | ------------------- | --- |
| GET    | /api/ipaddress      | To know the external IPAddress of the server |
| POST    | /api/register      | To register the first owner |
| GET    | /api/login          | To login an owner |
| POST   | /api/admin          | It's used for upgrading a user to owner |
| GET    | /api/admin/confirm  | It's used as a confirmation of the upgrade of the user to owner |
| POST   | /api/user           | To Register a new user into the database |
| POST   | /api/user/biometric | To initiate the process of setting the face and fingerprint data) |
| GET    | /api/user/:userid/images  | To retrieve a list with all the photo URIs of that user's face |
| GET    | /api/user/:userid/face/:imageid | To get the face image of a user |
| GET    | /api/user/:userid/profile/image | To get the profile image of a user |
| POST    | /api/user/:userid/profile/image | To upload a new profile image of a user |
| GET    | /api/users          | To retrieve a list with information about every user |
| GET    | /api/video          | To get the current snapshot/image back from the video streaming camera |
| GET    | /api/door           | To open the door of the locking device for 3 seconds |
| GET    | /api/logs            | To retrieve a list of logs from the house by a given range of dates |
| GET    | /api/notifications  | To retrieve new logs for displaying in notifications |
| PUT    | /api/notification/:notification_id | To mark the notification as read |

## Installation

```bash
sudo apt update && sudo apt install curl

bash <(curl -s https://raw.githubusercontent.com/ivancolomer/api-reac-android/master/install.sh)
```

# RestAPI Documentation

**Packet Format**
----
  The packet format is in JSON. There is a basic struct for all the packets returned by the API:
```javascript
{
  "error": true,
  "errorMessage": "expired_session_id",
  "content": 0
}
```
   Where `error` is a `boolean` type which tells if the request succeed or not.<br />
   Where `errorMessage` is a `string` type which is `null` if there isn't any error, or the error's identifier if there was an error.<br />
   Where `content` is a JSONObject which it's used to display different things depending on the request. When there's an error, it's    always 0.<br />

<a name="ip_address_extra"/>

**IPAddress**
----
  Returns a String on `content` field with the External IPAddress of the Server.

* ***URL***

  /api/ipaddress

* **Method:**

  `GET`
  
* **URL Params**
 
  None

* **Data Params**

  None

* **Success Response:**

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": false,
      "errorMessage": "",
      "content": "5.186.124.216"
    }
    ```
 
* **Error Response:**

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "unable_get_ipaddress",
      "content": 0
    }
    ```


<a name="register_administrator_extra"/>

**Register Administrator**
----
   Register a new Administrator when there isn't any (on first setup) and returns a RegisterResponse object on `content` field with the new generated random password and the id of the member.

* ***URL***

  /api/register

* **Method:**

  `POST`
  
* **URL Params**

  None

* **Data Params**

  **Required:**
 
   `serial_id=[string]` the serial code of the server device (from factory).<br />
   `user_name=[string]` the name of the new administrator.

* **Success Response:**

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
        "error": false,
        "errorMessage": "",
        "content": {
            "password": "HnP0BkORqL08ocPtddb8HQJmx3MH0UXMLG7FoiRDQEA=",
            "id": 51
        }
    }
    ```
 
* **Error Response:**

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "missing_request_parameters",
      "content": 0
    }
    ```

  OR

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "short_username_length",
      "content": 0
    }
    ```

  OR

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "database_error",
      "content": 0
    }
    ```

  OR

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "admin_already_exists",
      "content": 0
    }
    ```

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "wrong_serial_id",
      "content": 0
    }
    ```

  OR

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "name_already_in_use",
      "content": 0
    }
    ```

  OR

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "member_is_already_an_admin",
      "content": 0
    }
    ```


<a name="login_administrator_extra"/>

**Login Administrator**
----
   Used for login with your admin account and returns a String on `content` field with the new generated random session_id. This session_id will be valid during the following 15 minutes. Once this time has passed, it will not be longer valid and all the calls using this session_id will get an expired_session_id error.

* ***URL***

  /api/login

* **Method:**

  `GET`
  
* **URL Params**

  **Required:**
 
   `user_name=[string]` the name of the administrator account.<br />
   `password=[string]` the password of the administrator account (in Base64).

* **Data Params**

  None

* **Success Response:**

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
        "error": false,
        "errorMessage": "",
        "content": "F9-20-F6-89-6E-3E-76-4D-A3-20-EB-5F-74-F9-99-50"
    }
    ```
 
* **Error Response:**

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "missing_request_parameters",
      "content": 0
    }
    ```

  OR

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "short_username_length",
      "content": 0
    }
    ```

  OR

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "wrong_user_password",
      "content": 0
    }
    ```


<a name="user_to_admin"/>

**User To Admin**
----
   It's used for upgrading a member to administator/owner level. Returns a string on `content` field with the session_id needed for completing the upgrade. This session_id will expire within the next 5 minutes. A way to send the url from Android to Android has to be coded (via QR maybe??). The url sent should be: http://<SERVER_IP>/api/admin/confirm?session_id=<SESSION_ID>

* ***URL***

  /api/admin

* **Method:**

  `POST`
  
* **URL Params**

   None

* **Data Params**

  **Required:**

  `user_id=[uint]` the id of the user to upgrade ownership. <br />
  `session_id=[string]` the logged-in session_id.

* **Success Response:**

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": false,
      "errorMessage": "",
      "content": "F9-20-F6-89-6E-3E-76-4D-A3-20-EB-5F-74-F9-99-50"
    }
    ```
 
* **Error Response:**

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "missing_request_parameters",
      "content": 0
    }
    ```
    
  OR

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "expired_session_id",
      "content": 0
    }
    ```
        
  OR

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "member_is_already_an_admin",
      "content": 0
    }
    ```
        
  OR

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "database_error",
      "content": 0
    }
    ```
  
  
<a name="user_to_admin_confirm"/>

**User To Admin Confirm**
----
   It's used for upgrading a member to administator/owner level. Returns a RegisterResponse object on `content` field with the password of the new administrator and it's id. 

* ***URL***

  /api/admin/confirm

* **Method:**

  `GET`
  
* **URL Params**

   **Required:**

  `session_id=[string]` the new one time session_id generated on the previous step.

* **Data Params**

  None

* **Success Response:**

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": false,
      "errorMessage": "",
      "content": {
            "password": "HnP0BkORqL08ocPtddb8HQJmx3MH0UXMLG7FoiRDQEA=",
            "id": 51
        }
    }
    ```
 
* **Error Response:**

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "missing_request_parameters",
      "content": 0
    }
    ```
    
  OR

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "expired_session_id",
      "content": 0
    }
    ```
        
  OR

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "member_is_already_an_admin",
      "content": 0
    }
    ```
        
  OR

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "member_id_not_found",
      "content": 0
    }
    ```
    
     OR

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "database_error",
      "content": 0
    }
    ```


<a name="create_user"/>

**Create User**
----
   It's used for creating a new member. Returns a long integer on `content` field with the user_id needed of the new user.

* ***URL***

  /api/user

* **Method:**

  `POST`
  
* **URL Params**

   None

* **Data Params**

  **Required:**

  `user_name=[string]` the name of the new member. <br />
  `user_role=[string]` the role of the new member. <br />
  `session_id=[string]` the logged-in session_id.

* **Success Response:**

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": false,
      "errorMessage": "",
      "content": 13
    }
    ```
 
* **Error Response:**

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "missing_request_parameters",
      "content": 0
    }
    ```
    
  OR

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "expired_session_id",
      "content": 0
    }
    ```
        
  OR

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "short_username_length",
      "content": 0
    }
    ```
        
  OR

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "name_already_in_use",
      "content": 0
    }
    ```
  
  OR

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "database_error",
      "content": 0
    }
    ```


<a name="start_bio"/>

**Start Biometric Data User**
----
   It's used for signaling the locker device to start the biometric data process. Returns a string on `content` field with value 'biometric_process_has_begun'.

* ***URL***

  /api/user/biometric

* **Method:**

  `POST`
  
* **URL Params**

   None

* **Data Params**

  **Required:**

  `user_id=[uint]` the id of the user to start the process of biometric data. <br />
  `session_id=[string]` the logged-in session_id.

* **Success Response:**

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": false,
      "errorMessage": "",
      "content": "biometric_process_has_begun"
    }
    ```
 
* **Error Response:**

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "missing_request_parameters",
      "content": 0
    }
    ```
    
  OR

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "expired_session_id",
      "content": 0
    }
    ```
        
  OR

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "database_error",
      "content": 0
    }
    ```
        
  OR

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "locker_device_not_found",
      "content": 0
    }
    ```


<a name="user_images_list"/>

**User Biometric Images List**
----
   Retrieves a list with all the links of the user's face images. Returns an array of strings (links) on `content` field.

* ***URL***

  /api/user/:userid/images

* **Method:**

  `GET`
  
* **URL Params**

  **Required:**
 
   `:userid=[uint]` the name of the user. <br />
   `session_id=[string]` the logged-in session_id.

* **Data Params**

  None

* **Success Response:**

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
        "error": false,
        "errorMessage": "",
        "content": [ "/api/user/2/face/1", "/api/user/2/face/2", "/api/user/2/face/3" ]
    }
    ```
 
* **Error Response:**

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "missing_request_parameters",
      "content": 0
    }
    ```

  OR

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "expired_session_id",
      "content": 0
    }
    ```

  OR

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "database_error",
      "content": 0
    }
    ```

  OR

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "no_images_found",
      "content": 0
    }
    ```
    
  OR

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "locker_device_not_found",
      "content": 0
    }
    ```


<a name="user_image"/>

**User Face Image**
----
   It's used for downloading an face image from the server. Returns a response with Content-Type set to "image/png".

* ***URL***

  /api/user/:userid/face/:imageid

* **Method:**

  `GET`
  
* **URL Params**

  **Required:**
 
   `userid=[uint]` the id of the user to get the photo from. <br />
   `imageid=[uint]` the id of the face photo. <br />
   `session_id=[string]` the logged-in session_id.

* **Data Params**

  None

* **Success Response:**

  * **Code:** 200 <br />
    **Content:** 
    ```
    Content-Type: "image/png"
    ```
 
* **Error Response:**

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "missing_request_parameters",
      "content": 0
    }
    ```

  OR

   * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "expired_session_id",
      "content": 0
    }
    ```

  OR

   * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "database_error",
      "content": 0
    }
    ```

  OR

   * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "no_image_found",
      "content": 0
    }
    ```


<a name="user_profile_image"/>

**User Profile Image**
----
   It's used for downloading the profile image from the server. Returns a response with Content-Type set to "image/png" or "image/jpeg".

* ***URL***

  /api/user/:userid/profile/image

* **Method:**

  `GET`
  
* **URL Params**

  **Required:**
 
   `userid=[uint]` the id of the user to get the photo from. <br />
   `session_id=[string]` the logged-in session_id.

* **Data Params**

  None

* **Success Response:**

  * **Code:** 200 <br />
    **Content:** 
    ```
    Content-Type: "image/png"
    ```
 
* **Error Response:**

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "missing_request_parameters",
      "content": 0
    }
    ```

  OR

   * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "expired_session_id",
      "content": 0
    }
    ```

  OR

   * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "database_error",
      "content": 0
    }
    ```


<a name="upload_user_profile_image"/>

**Upload User Profile Image**
----
   It's used for uploading a profile image to the server. Returns a response with a string message "image_uploaded".

* ***URL***

  /api/user/:userid/profile/image

* **Method:**

  `POST`
  
* **URL Params**

  **Required:**
 
   `userid=[uint]` the id of the user to get the photo from. <br />
   `session_id=[string]` the logged-in session_id.

* **Data Params**

   **Required:**
 
    `file=[file]` the photo to be sent to the server. <br />

* **Success Response:**

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": false,
      "errorMessage": "",
      "content": "image_uploaded"
    }
    ```
 
* **Error Response:**

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "missing_request_parameters",
      "content": 0
    }
    ```

  OR

   * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "expired_session_id",
      "content": 0
    }
    ```

  OR

   * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "database_error",
      "content": 0
    }
    ```

  OR

   * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "no_file_uploaded",
      "content": 0
    }
    ```


<a name="users_list"/>

**Users List**
----
   Retrieves a list with information about every member. Returns an array of JObject on `content` field with information about all the users registered in the database (owners and members).

* ***URL***

  /api/users

* **Method:**

  `GET`
  
* **URL Params**

  **Required:**
 
   `session_id=[string]` the logged-in session_id.

* **Data Params**

  None

* **Success Response:**

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
        "error": false,
        "errorMessage": "",
        "content": [
            {
                "isOwner": true,
                "userID": 1,
                "name": "ivan",
                "role": "ADMIN",
                "profilePhoto": "/api/user/1/profile/image"
            },
            {
                "isOwner": true,
                "userID": 6,
                "name": "ivan2",
                "role": "ADMIN",
                "profilePhoto": "/api/user/2/profile/image"
            },
            {
                "isOwner": false,
                "userID": 8,
                "name": "Test1",
                "role": "MEMBER",
                "profilePhoto": "/api/user/3/profile/image"
            }
        ]
    }
    ```
 
* **Error Response:**

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "missing_request_parameters",
      "content": 0
    }
    ```

  OR

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "expired_session_id",
      "content": 0
    }
    ```

  OR

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "database_error",
      "content": 0
    }
    ```

    
<a name="camera_streaming"/>

**Camera Streaming**
----
   It's used for getting the image in `jpeg` format from the video camera.

* ***URL***

  /api/video

* **Method:**

  `GET`
  
* **URL Params**

   **Required:**

  `session_id=[string]` the session_id from the current session from an administrator.

* **Data Params**

  None

* **Success Response:**

  * **Code:** 200 <br />
    **Content:** 
    ```
    Content-Type: "image/jpeg"
    ```
 
* **Error Response:**

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "missing_request_parameters",
      "content": 0
    }
    ```
    
  OR

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "expired_session_id",
      "content": 0
    }
    ```
        
  OR

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "unable_get_ipaddress",
      "content": 0
    }
    ```
    
  OR

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "no_live_image_available",
      "content": 0
    }
    ```
        

<a name="open_door"/>

**Open Door**
----
   It's used for opening the lock door for 3 seconds. Returns an empty string on `content`.

* ***URL***

  /api/door

* **Method:**

  `GET`
  
* **URL Params**

   **Required:**

  `session_id=[string]` the session_id from the current session from an administrator.

* **Data Params**

  None

* **Success Response:**

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": false,
      "errorMessage": "",
      "content": [
        "door_opened"
      ]
    }
    ```
 
* **Error Response:**

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "missing_request_parameters",
      "content": 0
    }
    ```
    
  OR

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "expired_session_id",
      "content": 0
    }
    ```
        
  OR

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "locker_device_not_found",
      "content": 0
    }
    ```

<a name="get_log"/>

**Get Log**
----
   It's used for retrieving logs. Returns a list of `log` objects on `content`.

* ***URL***

  /api/logs

* **Method:**

  `GET`
  
* **URL Params**

   **Required:**

  `session_id=[string]` the session_id from the current session from an administrator. <br />
  `begin_date=[date:string]` the date since the logs should be displayed. Format: `dd/MM/yyyy-HH:mm:ss` (in UTC) ex: `31/05/2019-14:03:43`<br />
  `end_date=[date:string]` the date until the logs should be displayed. Format: `dd/MM/yyyy-HH:mm:ss` (in UTC) ex: `31/05/2019-14:03:43`

* **Data Params**

  None

* **Success Response:**

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
        "error": false,
        "errorMessage": "",
        "content": [
            {
                "logID": 1,
                "userID": 1,
                "name": "ivan",
                "profilePhoto": "/api/user/1/profile/image",
                "date": "10/12/2019-11:47:13",
                "info": "user_to_owner"
            },
            {
                "logID": 2,
                "userID": 1,
                "name": "ivan",
                "profilePhoto": "/api/user/2/profile/image",
                "date": "10/12/2019-12:35:23",
                "info": "button_open_door"
            },
            {
                "logID": 3,
                "userID": 1,
                "name": "ivan",
                "profilePhoto": "/api/user/3/profile/image",
                "date": "10/12/2019-12:36:17",
                "info": "open_door"
            }
        ]
    }
    ```
 
* **Error Response:**

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "missing_request_parameters",
      "content": 0
    }
    ```
    
  OR

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "expired_session_id",
      "content": 0
    }
    ```
        
  OR

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "database_error",
      "content": 0
    }
    ```

<a name="get_notification_log"/>

**Get Notification Log**
----
   It's used for pulling new notifications. Returns a a list of `log` objects on `content`.

* ***URL***

  /api/notifications

* **Method:**

  `GET`
  
* **URL Params**

   **Required:**

  `session_id=[string]` the session_id from the current session from an administrator.

* **Data Params**

  None

* **Success Response:**

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
        "error": false,
        "errorMessage": "",
        "content": [
            {
                "logID": 1,
                "userID": 1,
                "name": "ivan",
                "profilePhoto": "/api/user/1/profile/image",
                "date": "10/12/2019-11:47:13",
                "info": "user_to_owner"
            },
            {
                "logID": 2,
                "userID": 1,
                "name": "ivan",
                "profilePhoto": "/api/user/2/profile/image",
                "date": "10/12/2019-12:35:23",
                "info": "button_open_door"
            },
            {
                "logID": 3,
                "userID": 1,
                "name": "ivan",
                "profilePhoto": "/api/user/3/profile/image",
                "date": "10/12/2019-12:36:17",
                "info": "open_door"
            }
        ]
    }
    ```
 
* **Error Response:**

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "missing_request_parameters",
      "content": 0
    }
    ```
    
  OR

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "expired_session_id",
      "content": 0
    }
    ```
        
  OR

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "database_error",
      "content": 0
    }
    ```


<a name="set_notification_log_read"/>

**Set Notification Log Read**
----
    It's used for marking the notification as read. Returns an empty string on `content`.

* ***URL***

  /api/notification/:notification_id

* **Method:**

  `PUT`
  
* **URL Params**

   **Required:**

  `session_id=[string]` the session_id from the current session from an administrator.
  `notification_id=[int]` the log id of the notification you want to mark it as read.

* **Data Params**

  None

* **Success Response:**

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": false,
      "errorMessage": "",
      "content": ""
    }
    ```
 
* **Error Response:**

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "missing_request_parameters",
      "content": 0
    }
    ```
    
  OR

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "expired_session_id",
      "content": 0
    }
    ```
        
  OR

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "database_error",
      "content": 0
    }
    ```
