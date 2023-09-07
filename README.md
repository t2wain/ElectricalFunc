# Electrical Libraries

This repository consists of several libraries that perform various engineering calculations such as voltage drop, NEC tray fill, and cable routing. These are some of the features in applications that I have developed over the years. I have recoded these features many times as an exercise to apply the latest software design practices. The redesign this time is to apply the principles of functional programming.

There are four (4) projects that implement the main features:
- VDropLib
- GraphLib
- NECFillLib
- RouteLib

The other three (3) projects are testing projects:
- AppConsole
- RouteDB
- UnitTestroject

# Functional Programming Principles

Belows are some of the principles I tried my best to follow in this code redesign:
- Object only contains data (no method)
- Object is immutable. All modifications return new objects.
- Minimize the use of primitive data type as argument. Use simple object instead to convey semantic meaning.
- Functions implement behaviors of objects (not methods).
- Implement pure functions with no side effect
  - The return value will depend only on the function arguments.
  - The function will not modify the external environment.
- Function is referential transparent
  - It always returns the same output for the same inputs. 
- Implement partial application (currying) with the use of higher order function
  - A function that return another function.
  - Leverage the closure principle to capture state for a function
  - As a design to reduce the number of arguments of a function and create a simpler function to use.
- Rail programming
  - The output of a function is the input of the next function. This call chain should continue to completion even when there is an error.
  - Within each function, there should be branches for success and failure to allow for continuation.

# Functional Language Features in C# 10

- Use immutable record instead of class to implement object
- Pattern-matching statement and expression
- Object and tuple destructuring
- Copy constructor for cloning immutable object
- Inner function
- Extension method
- LINQ
  
