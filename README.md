# Expense Tracker
An Expense Tracker application built with ASP.NET Core 8 and SQL Server, providing user and admin roles for managing transactions, categories, and insightful financial reports. The application includes features like email verification, password recovery, and Excel export functionality.

Features

## Authentication & Authorization

	User Registration with Email Verification

	Forgot Password / Reset Password functionality

	Role-based Authorization:

•	Admin: Full access to categories and transactions

•	User: Can view categories and manage transactions

## Category Management

	Only Admins can Create, Read, Update, and Delete (CRUD) categories

	Users can view categories but cannot modify them

## Transaction Management

	Both Admin and User can perform CRUD operations on transactions.

	Search transactions using date range (start date and end date)

	Export transactions as an Excel file:

•	Download all transactions.

•	Download transactions filtered by a date range.

## Dashboard

	View comprehensive financial reports:

•	Weekly, Monthly, and Yearly Reports

•	Summary Report of expenses

o	Pie Chart: Visualize expenses by category.

o	Spline Chart: Compare income vs expenses.

## User Profile Management

	Update Profile Details (name, email, phone, address, etc.)

	Change Password.

# Technologies Used

	Backend: ASP.NET Core 8

	Database: SQL Server

	Frontend: Razor Pages, Bootstrap 5

	Charts: Chart.js (for Pie and Spline charts)

	Excel Export: EPPlus and XML (for generating Excel files)

	Email Services: SMTP (for email verification and reset password)

# Installation & Setup

Step 1 # Installation

[Visual Studio 2022+](https://visualstudio.microsoft.com/)

[SQL Server](https://www.microsoft.com/en-us/sql-server/)

Step 2 # Clone the Repository

```bash
https://github.com/zuezuemaung310/ExpenseTrackerSecure.git
```

```bash
cd ExpenseTracker
```

Step 3 # Create and Configure the Database in SQL Server

```bash
CREATE DATABASE Your_Database;
```

Step 4 # Open Visual Studio 2022, navigate to your project folder, and select the ExpenseTracker.sln solution file to open the project.

Step 5 # Update Connection String in appsettings.json

```bash
"ConnectionStrings": {
    "DefaultConnection": "Server=Your_Server;Database=Your_Database;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

Step 6 # Tools from NuGet Package Manager and Package Manager Console

Step 7# Apply Database Migrations on Package Manager Console
```bash
Add-Migration Initial
```

```bash
Update-Database
```
# If you wanna use MySQL instead of SQL Server

Step 1# Tools from NuGet Package Manager and Manage NuGet Packages for Solution

Step 2 # Install MySQL in NuGet Package Manager
```bash
MySql.EntityFrameworkCore
```

Step 3# Update Connection String in appsettings.json
```bash
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=your-database-name;User=root;Password=your-password;"
}
```



## System Flow Chart
![Flow Chart](https://github.com/user-attachments/assets/f07afb12-c3b9-4986-993e-b9857321aca3)


##Website Design

#Register Page
![register](https://github.com/user-attachments/assets/bb765201-debe-4a46-a3c1-0a8b7c13b14e)

#Login Page
![login](https://github.com/user-attachments/assets/af06b6ca-e81b-4dd8-83dd-9a0c7631ea4d)

#Forgot Password Page
![forgot](https://github.com/user-attachments/assets/081f6e66-14ae-4eeb-b481-d2755a07292d)

#Profile Page
![profile](https://github.com/user-attachments/assets/f73edc30-1c8f-40a4-8526-1b7435df9b5a)

#Dashboard Page
![dashboard](https://github.com/user-attachments/assets/62d6bff6-64ec-437b-bc91-db1dc0869179)

#Category Page
![category](https://github.com/user-attachments/assets/7ba26d39-d9d3-4327-b13c-d045260405cc)

#Transaction Page
![transaction](https://github.com/user-attachments/assets/ceb39d3b-6a95-44d9-a587-a8d7e2ab4d9d)




