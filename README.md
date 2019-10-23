# api-reac-android

This API will be used as a middleware between the Android Application and the Database/Logic of the server.
The API will be RESTful and their CRUD operations are defined below:

| Method | Url | Action |
| ------ | ------------------- | --- |
| POST   | /login              | Checks if the given password is good, and if so, returns a SESSION_ID used for the following queries |
| POST   | /register           | Registers a new user into the database |
| GET    | /user               | Retrieve all the information about the user given an id and SESSION_ID (logged-in) |
| PUT    | /user               | Updates information about the logged-in user (name, profile photo, ...) |
| POST   | /user/fingerprint   | Saves the fingerprint photo sent by the user with id 1 used for the fingerprint recognition algorithm |
| POST   | /user/face          | Saves the face photo sent by the user with id 1 used for the face recognition algorithm |
| POST   | /friend             | Send a request to add him/her to my list of friends by mail address or mobile number (it can be family too, there will be different categories)
| PUT    | /friend             | Accepts or decline friendship request |
| GET    | /friends            | Retrieve a list of all the friends the logged-in user has |
| GET    | /friend/1           | Retrieve information about the friend with id 1 |
| DELETE | /friend/1           | Deletes the friend with id 1 from friends' list |
| POST   | /friend/1/enter     | Let the friend enter your home for some time defined by the user |
