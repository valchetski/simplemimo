# My notes
## How to run
From Visual Studio/Rider run the `SimpleMimo` project. It'll create a SQLite database and populate it with data on startup.
By default, it will return Open API documentation JSON.
In case you want to test the API choose `scalar` launch profile for `SimpleMimo` project.
It will start a Scalar UI which works similar to Swagger UI and will allow to send requests to the API.

## My assumptions
### Each lesson can be solved multiple times
Assumed that we need to track/save in db every lesson completion. Not that we need to save first completion and ignore the others.
### Achievements API
I've designed it in a way that it'll return only achievements that user started tracking.
If no lessons were completed, then no achievements will be returned.
If for example one lesson is completed from C# course then it will return all lessons, chapter and `Complete the C# course` achievements progress.
For course-related achievements the progress will be number of chapters that user completed in that course.
### Folder structure
I've add all database related logic to `Data` folder and all business logic to `Services` folder. In real-life those folders could be separate C# projects.
Didn't do it here for the simplicity.

### Database structure
The biggest concern for me was how to organize Lessons, Chapters and Courses in database.
#### First approach (the one I chose)
Have separate tables for Lessons, Chapters and Courses.
Lesson has a ChapterId foreign key. Chapter has CourseId foreign key.

Pros:
- Simple database structure.
- The code logic is relatively easy to understand.

Cons:
- Very similar code for Lessons, Chapters and Courses processing (you can see it in my code).
- Each entity needs a separate table for User relations: UserLessons, UserChapters and UserCourses.
#### Second approach
Create one table (we can call it Item) for Lessons, Chapters and Courses. 
Each Item will have:
- ItemType field (Lesson, Chapter or Course) 
- ParentId (for Lesson the value will be ID of Chapter, for Chapter ID of Course, for Course value if always NULL).
- Metadata. Will have data that specific/unique for each type. Can be in JSON format.

Pros:
- Less duplicate logic in the code.
- Presumably, logic will be easier to extend in case new item type needs to be added. For example, we need new Item that will group courses together.

Cons:
- Logic can be harder to understand.
- Database may become harder to maintain in the future. 
- Need additional logic that will deserialize Metadata JSON.
#### My conclusion
I chose the first approach as it's more straightforward. I think in real-life Lesson and Chapter/Courses entities will be very different. 
Chapters and Courses mainly act as a containers for Lessons and all helpful info will be in the Lesson table.
The second approach could be a better fit for the document database (for example, MongoDb) where everything stored in JSON-like format. And there much easier to have documents with a bit different structure in one collection.



# Mimo Backend Coding Challenge

Copied from: https://github.com/getmimo/coding-challenge/blob/4cc7b28b5addc48f206672c6698fdb8cf09ed2e5/backend-challenge.md

Your objective is to create a basic version of the Mimo server functionality.

The point of this exercise is not to create a fully-functional bug-free server implementation,
but rather to look how you would design such a server system and how you approach different problems.

Write the server with ASP.NET Core, backed by Entity Framework Core with SQLite as database
(so we don't have to setup a full blown database).

The server should expose only REST endpoints using JSON as the format.

## Data structure

### Courses

- There are multiple **courses** (create a "Swift", "Javascript" and "C#" course in the database)
- Each **course** has multiple **chapters**
- **Chapters** have a specific order in which they are displayed
- Each **chapter** has multiple **lessons**
- **Lessons** have a specific order in which they are displayed

### Users

- There are multiple **users** accessing the REST endpoints (e.g., from the Mimo app)
- You don't have to create an authentication system, just hardcode any user-specific actions to a single user
- Each user can solve multiple **lessons**
- Each **lesson** can be solved multiple times
- Track the time the **user** starts the **lessons** and the time the **user** completes the **lesson**

### Achievements

- There are **achievements** for completing different objectives (see below)
- Each **user** can complete each of the achievements below
- Once a user completes an **achievement**, mark it as completed

#### Objectives

- Complete 5 lessons
- Complete 25 lessons
- Complete 50 lessons
- Complete 1 chapter
- Complete 5 chapters
- Complete the Swift course
- Complete the Javascript course
- Complete the C# course

## API

### Lesson progress

Create an endpoint where the Mimo app can send information about lessons that the user completed to.
The app will send the following data:
- Which lesson was completed
- When was the lesson started
- When was the lesson completed

### Achievements

Create an endpoint where the Mimo app can request which achievements the users has completed
The app needs the following data:
- An identifier for the achievement
- If the achievement is completed or not
- What the progress of the achievement is (e.g. if the user solved three lessons,
  it should return "3" for the lesson completion achievements)