# Argo Sales Tracker

## Introduction

**Argo Sales Tracker** is a Windows app designed to help small to medium sized companies manage their finances. It was created out of the need for a one-time payment solution, as most existing sales trackers are subscription-based, very overpriced, and have a high learning curve. Argo Sales Tracker offers advanced features while remaining easy to use, filling the gap between basic spreadsheets and enterprise-level solutions.

## Features

- **Import data from spreadsheets**: import your existing sales, purchases, products, and other data from spreadsheets.
- **Track Purchases and Sales**: Add purchases and sales.
- **Return products**: Track returned products and the reasons they were returned.
- **Product management**: Add, edit, and organize your products with ease.
- **Receipts**: Attach receipts to your transactions and keep them organized.
- **Custom date ranges**: Filter transactions using custom date ranges.
- **Analytics dashboard**: Visualize your data using charts to gain insights into sales trends and performance.

## Technologies used

- **C# .NET 9**: Core framework for the application's logic and UI.
- **WinForms**: Windows Forms is used for the graphical interface.
- **Guna UI**: Enhances the look and feel of the application with modern controls and components.
- **LiveCharts**: Used to visualize data through beautiful and interactive charts.

## Prerequisites

- **.NET 9 SDK**: Make sure you have the latest .NET 9 SDK installed on your machine. You can download it [here](https://dotnet.microsoft.com/en-us/download/dotnet/9.0).
- **Visual Studio 2022 or later**: This project is developed using Visual Studio.
- **API Keys**: You'll need to obtain the .env file containing the API keys from Evan Di Placido.

## Installation

1. Clone the repository to your local machine.
2. Place the .env file in the project root directory (where the .sln file is located).
3. Build the project by pressing `Ctrl + Shift + B`.

## Running the application

After building the solution, press F5 to run the application in Debug mode, or `Ctrl + F5` to run it in Release mode.

To use the program effectively, follow these steps:

1. **Add companies**: Enter the companies you will be buying from.
2. **Add categories**: Organize your products by creating categories, making it easier to manage inventory and track sales. For example, if your company buys many different types of robotic components, you can add the categories "Bolts, nuts, screws, ball bearings, etc.
3. **Add products**: Once categories are set, you can create products within those categories.
4. **Add sales and purchases**: Now you can start adding purchases and sales into the system and watch the charts update in real time.

## Publishing Argo Sales Tracker to a .exe file
The `.exe` installer for Argo Sales Tracker is built using [Advanced Installer Professional Edition](https://www.advancedinstaller.com/) to ensure a smooth and professional installation experience.
Feel free to contact me directly to request the Advanced Installer project files. Note that the proffesional edition is a paid product.

