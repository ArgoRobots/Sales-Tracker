# Sales Tracker

## Introduction
**Argo Sales Tracker** is a simple, powerful, and effective C# WinForms application designed to track the sales and purchases of products. It was created out of the need for a one-time payment solution, as most existing sales trackers are subscription-based, overpriced, and overcomplicated. Argo Sales Tracker offers advanced features while remaining easy to use, filling the gap between basic spreadsheets (e.g., Excel or Google Sheets) and enterprise-level software.

## Importing data from spreadsheets
NOT IMPLEMENTED YET

To import your existing sales, purchase, or product data from a spreadsheet into the Argo Sales Tracker:

1. **Prepare Your Spreadsheet**: Ensure your data is organized in a structured manner. The columns should match the fields in the application (e.g., Product Name, Category, Date, Amount, etc.).
2. **Open the Import Tool**: From the main menu, navigate to "File > Import".
3. **Select Your Spreadsheet**: Choose the spreadsheet file that contains your existing data.
4. **Map Columns**: The application will prompt you to map the columns in your spreadsheet to the corresponding fields in Argo Sales Tracker.
5. **Complete Import**: After verifying the data mapping, click Import. Your data will now be added to the application, and you can begin tracking from where you left off.

## Features
- **Track Sales and Purchases**: Add and manage sales and purchases.
- **Accountants Integration**: Keep track of accountants working with the sales data.
- **Product Management**: Add, edit, and organize your products with ease.
- **Receipts and Exporting**: Export receipts and sales data for your records.
- **Custom Date Ranges**: Filter sales and purchases with custom date ranges.
- **Charts and Analytics**: Visualize your data using charts to gain insights into sales trends and performance.

## Technologies used
- **C# .NET 8**: Core framework for the application's logic and UI.
- **WinForms**: Windows Forms is used for the graphical interface.
- **Guna UI**: Enhances the look and feel of the application with modern controls and components.
- **Guna Charts**: Used to visualize data through beautiful and interactive charts.

## Prerequisites
- **.NET 8 SDK**: Make sure you have the latest .NET 8 SDK installed on your machine. You can download it [here](https://dotnet.microsoft.com/en-us/download/dotnet/8.0).
- **Visual Studio 2022 or later**: This project is developed using Visual Studio.

## Installation
1. Clone the repository to your local machine using the following command:
   ```bash
   git clone <repository-url>
2.	Open the solution file "Sales Tracker.sln" in Visual Studio.
3.	Restore the NuGet packages by going to Tools > NuGet Package Manager > Restore NuGet Packages.
4.	Build the project by pressing Ctrl + Shift + B.

## Running the application
After building the solution, press F5 to run the application in Debug mode, or Ctrl + F5 to run it in Release mode.
Initial Setup

To use the program effectively, follow these steps:
1.	**Add an accountant**: The first step is to add an accountant that will be associated with the sales and purchases.
2.	**Add companies**: Enter the companies you will be buying from.
3.	**Add categories**: Organize your products by creating categories, making it easier to manage inventory and track sales. For example, if your company buys many different types of robotic components, you can add the categories "Bolts, nuts, screws, ball bearings, motors, etc.
4.	**Add products**: Once categories are set, you can create products within those categories.
5.	**Add sales and purchases**: After setting up accountants, companies, categories, and products, you can start adding sales and purchases into the system.
