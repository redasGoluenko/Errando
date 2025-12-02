-- Create Users table
CREATE TABLE IF NOT EXISTS "Users" (
    "Id" SERIAL PRIMARY KEY,
    "Username" VARCHAR(100) NOT NULL UNIQUE,
    "PasswordHash" TEXT NOT NULL,
    "Role" VARCHAR(50) NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Create Tasks table
CREATE TABLE IF NOT EXISTS "Tasks" (
    "Id" SERIAL PRIMARY KEY,
    "Title" VARCHAR(200) NOT NULL,
    "Description" TEXT,
    "Location" TEXT,
    "Status" VARCHAR(50) NOT NULL DEFAULT 'Pending',
    "RunnerId" INTEGER,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY ("RunnerId") REFERENCES "Users"("Id") ON DELETE SET NULL
);

-- Create TaskItems table
CREATE TABLE IF NOT EXISTS "TaskItems" (
    "Id" SERIAL PRIMARY KEY,
    "TaskId" INTEGER NOT NULL,
    "ItemName" VARCHAR(200) NOT NULL,
    "IsCompleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY ("TaskId") REFERENCES "Tasks"("Id") ON DELETE CASCADE
);

-- Create StatusLogs table
CREATE TABLE IF NOT EXISTS "StatusLogs" (
    "Id" SERIAL PRIMARY KEY,
    "TaskId" INTEGER NOT NULL,
    "StatusMessage" TEXT NOT NULL,
    "LoggedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY ("TaskId") REFERENCES "Tasks"("Id") ON DELETE CASCADE
);

-- Insert admin user (password: admin123)
INSERT INTO "Users" ("Username", "PasswordHash", "Role", "CreatedAt")
VALUES ('admin', '$2a$11$xhKZQ8vZ9QXJ5Y3f7Xv5DOYvKr0WYjCmPqN/8WXJ5Y3f7Xv5DOYvK', 'Admin', CURRENT_TIMESTAMP)
ON CONFLICT ("Username") DO NOTHING;