# Web API for an online Delivery Restaurant

## Main Features :

- **Account Handler**  :  `A general user account implementation with security pracites` accessible at `/api/Account/[]`
  - Register `POST/api/Account/register`
  - Login `POST/api/Account/login`
  - Logout `POST/api/Account/logout`
  - Get Profile data `GET/api/Account/profile`
  - Multiplied Admin Account Creation Support `POST/api/Account/register` with parameter **adminSecretKey**

- **Dish items Handler**   `Implementing Food Dish details and data as required` accessible at `/api/Dish/[]`
  - List all dishes with available **Sorting and Filters**  `GET : /api/Dish`
  - Getting info about specific Dish by ``id``  `PUT : /api/Dish/{id}`
  - Check Rating of A dish by `id` `GET : /api/Dish/{id}/rating/check`
  - Putting a rating to Dish by `id`   `POST: /api/Dish/{id}/rating`
  - **Admin** : Creating a new Dish item in db `PUT: /api/Dish`



## Requirements:

- in file **/appsettings.json** 
  - set `JwtSettings:Secret` for **JWT** authentication system secret token 
  - set `Admin:Secret` for general Admin access over the application and to be able to create Admin Users `Account Controller -> Register -> adminSecretKey`