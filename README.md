# MyPushBox

A classical game and its AI path finding algorithm.
\
See project report at **./Report.pdf**

## Installation

### Environment Requirement

+ OS: **Windows 10**
+ CPU: **x86/x64/ARM**

### Steps

    cd AppPackages/MyPushBox_1.0.1.0_Test
    ./Install.ps1

## Operating Instructions

## Source Files

The project is a UWP application written in C#. XAML files are used to layout the UI. Here are the source files.

+ ./App.xaml
  + Application layout settings. Nothing important here.
+ ./App.xaml.cs
  + Application general settings and program intrance. Nothing important here.
+ ./src/
  + MainPage.xaml
    + Layout of the only page in the application.
  + MainPage.xaml.cs
    + UI event handlers.
  + BoxGame.cs
    + Game logic and other definations.
  + Utils.cs
    + General components, including an implement of priority queue(heap).
  + AIPlayer.cs
    + Search algorithm(A\* algorithm).
  + Hungary.cs
    + Hungary algorithm, used to match boxes and targets to calculate distance in the AI algorithm.
