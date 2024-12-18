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
git clone https://github.com/your-username/your-repository.git
cd your-repository

cd ExpenseTracker

Step 3 # Create and Configure the Database in SQL Server

CREATE DATABASE Your_Database;

Step 4 # Open Visual Studio 2022, navigate to your project folder, and select the ExpenseTracker.sln solution file to open the project.

Step 5 # Update Connection String in appsettings.json

"ConnectionStrings": {
    "DefaultConnection": "Server=Your_Server;Database=Your_Database;Trusted_Connection=True;TrustServerCertificate=True;"
}

Step 6 # Tools from Node Package Manager and Package Manager Console

Step 7# Apply Database Migrations on Package Manager Console

Add-Migration Initial

Update-Database


## System Flow Chart
![Flow Chart](https://github.com/user-attachments/assets/f07afb12-c3b9-4986-993e-b9857321aca3)


##Website Design

#Register Page
![register](https://github.com/user-attachments/assets/5de41e12-52bc-433b-be29-d823a47f2ef8)

#Login Page
![login](https://github.com/user-attachments/assets/8120c6c8-b1d2-4df5-8925-7ab205e56802)

#Profile Page
![login](https://github.com/user-attachments/assets/8120c6c8-b1d2-4df5-8925-7ab205e56802)

#Dashboard Page
![dashboard](https://github.com/user-attachments/assets/a51d3d90-bf2b-4c00-81af-4a87a33c6aba)

#Category Page
![category](https://github.com/user-attachments/assets/0bb277c2-5e02-4cfd-97de-e6e586178e72)

#Transaction Page
![transaction](https://github.com/user-attachments/assets/16a0894c-328e-47a1-af51-37bb2a1621bd)






