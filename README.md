# Web API for an online Delivery Restaurant

## Main Features :

- **Account Handler**  :  

  controller : `AccountController.cs` 
  Service for db_operations : `AuthService.cs `

  `A general user account implementation with security pracites` accessible at `/api/Account/[]`

  - Register `POST/api/Account/register`
  - Login `POST/api/Account/login`
  - Logout `POST/api/Account/logout`
  - Get Profile data `GET/api/Account/profile`
  - **Admin** : Multiplied Admin Account Creation Support `POST/api/Account/register` with parameter **adminSecretKey**

- **Dish items Handler**   

  controller : `DishController.cs` 
  Service for db_operations : `DishService.cs `

  `Implementing Food Dish details and data as required` accessible at `/api/Dish/[]`

  - List all dishes with available **Sorting and Filters**  `GET : /api/Dish`
  - Getting info about specific Dish by ``id``  `PUT : /api/Dish/{id}`
  - Check Rating of A dish by `id` `GET : /api/Dish/{id}/rating/check`
  - Putting a rating to Dish by `id`   `POST: /api/Dish/{id}/rating`
  - **Admin** : Creating a new Dish item in db `PUT: /api/Dish`

- **Dish Rating system**  

  `a rating system following logical rating system by calc average ratings of all users for a specific dish`

  `affecting` `GET : /api/Dish/{id}/rating/check` , `POST: /api/Dish/{id}/rating` ,`GET : /api/Dish` 

- **Business_logic_Security_Mitigations** : 

  - Same **user** cant put more than **one rating** on **same dish** . handled at  `DishService.cs` 
  - Same **user** cant be **registered** **twice**. handled at `AuthService.cs`
  - normal user cant create dish at `PUT: /api/Dish` . handled at ` AuthService.cs`

## Setup:

- in file **/appsettings.json** 
  - set `JwtSettings:Secret` for **JWT** authentication system secret token 
  - set `Admin:Secret` for general Admin access over the application **adminSecretKey** and to be able to create Admin Users `Account Controller -> Register -> adminSecretKey`
  - **Postgresql_config** : set values of `Host=;Database=;Username=;Password=;` according to your db user and database name on which you will work on
- **Migrate** files :
  - if Migration folder is empty or migration files show errors . then delete them and then you may exec in **cmd** in project folder : ` dotnet ef migrations add migration_name` to initiate a migration to be executed in db   
  - **update database structure** to create required tables and structure with **cmd** with command : `dotnet ef database update`
- **Run!**