-- ============================================================
--  AcadSystem — Full Database Setup + Sample Data
--  Run once in MySQL Workbench or phpMyAdmin (XAMPP)
--  To re-run cleanly: DROP DATABASE acadsystem; then run this.
-- ============================================================

CREATE DATABASE IF NOT EXISTS acadsystem;
USE acadsystem;

-- ============================================================
--  SCHEMA
-- ============================================================

CREATE TABLE IF NOT EXISTS users (
    user_id       INT AUTO_INCREMENT PRIMARY KEY,
    username      VARCHAR(50)  NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    full_name     VARCHAR(100) NOT NULL,
    email         VARCHAR(100) NOT NULL UNIQUE,
    role          ENUM('Admin','Staff','Instructor') NOT NULL DEFAULT 'Staff',
    is_active     TINYINT(1)   NOT NULL DEFAULT 1,
    created_at    DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at    DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS departments (
    department_id   INT AUTO_INCREMENT PRIMARY KEY,
    department_name VARCHAR(100) NOT NULL,
    office_location VARCHAR(100)
);

CREATE TABLE IF NOT EXISTS instructors (
    instructor_id INT AUTO_INCREMENT PRIMARY KEY,
    first_name    VARCHAR(50)  NOT NULL,
    last_name     VARCHAR(50)  NOT NULL,
    email         VARCHAR(100),
    department_id INT,
    FOREIGN KEY (department_id) REFERENCES departments(department_id)
);

CREATE TABLE IF NOT EXISTS courses (
    course_id     INT AUTO_INCREMENT PRIMARY KEY,
    course_name   VARCHAR(100) NOT NULL,
    credits       INT,
    department_id INT,
    instructor_id INT,
    FOREIGN KEY (department_id) REFERENCES departments(department_id),
    FOREIGN KEY (instructor_id) REFERENCES instructors(instructor_id)
);

CREATE TABLE IF NOT EXISTS students (
    student_id    INT AUTO_INCREMENT PRIMARY KEY,
    first_name    VARCHAR(50)  NOT NULL,
    last_name     VARCHAR(50)  NOT NULL,
    email         VARCHAR(100),
    date_of_birth DATE
);

CREATE TABLE IF NOT EXISTS enrollments (
    enrollment_id   INT AUTO_INCREMENT PRIMARY KEY,
    student_id      INT NOT NULL,
    course_id       INT NOT NULL,
    enrollment_date DATE,
    grade           CHAR(2),
    FOREIGN KEY (student_id) REFERENCES students(student_id),
    FOREIGN KEY (course_id)  REFERENCES courses(course_id)
);

-- ============================================================
--  USER ACCOUNTS
--  Each instructor below has a matching record in the
--  instructors table (same name, same email).
--  All passwords: admin123 for admin, password123 for others.
-- ============================================================

-- Admin
INSERT IGNORE INTO users (username, password_hash, full_name, email, role, is_active) VALUES
('admin', SHA2('admin123', 256), 'System Administrator', 'admin@acadsystem.edu', 'Admin', 1);

-- Staff (registrar office)
INSERT IGNORE INTO users (username, password_hash, full_name, email, role, is_active) VALUES
('r.dela_torre', SHA2('password123', 256), 'Rosa Dela Torre',  'r.delatorre@acadsystem.edu', 'Staff', 1),
('c.santos',     SHA2('password123', 256), 'Carlos Santos',    'c.santos@acadsystem.edu',    'Staff', 1);

-- Instructors — names and emails match the instructors table exactly
INSERT IGNORE INTO users (username, password_hash, full_name, email, role, is_active) VALUES
('j.reyes',       SHA2('password123', 256), 'Jose Reyes',       'j.reyes@acadsystem.edu',       'Instructor', 1),
('m.santos',      SHA2('password123', 256), 'Maria Santos',     'm.santos@acadsystem.edu',      'Instructor', 1),
('a.cruz',        SHA2('password123', 256), 'Ana Cruz',         'a.cruz@acadsystem.edu',        'Instructor', 1),
('r.garcia',      SHA2('password123', 256), 'Roberto Garcia',   'r.garcia@acadsystem.edu',      'Instructor', 1),
('l.bautista',    SHA2('password123', 256), 'Lourdes Bautista', 'l.bautista@acadsystem.edu',    'Instructor', 1),
('e.delacruz',    SHA2('password123', 256), 'Emmanuel Dela Cruz','e.delacruz@acadsystem.edu',   'Instructor', 1),
('p.villanueva',  SHA2('password123', 256), 'Patricia Villanueva','p.villanueva@acadsystem.edu','Instructor', 1),
('r.flores',      SHA2('password123', 256), 'Ramon Flores',     'r.flores@acadsystem.edu',      'Instructor', 0);

-- ============================================================
--  DEPARTMENTS
-- ============================================================

INSERT IGNORE INTO departments (department_id, department_name, office_location) VALUES
(1, 'Information Technology',  'Building A, Room 101'),
(2, 'Computer Science',        'Building A, Room 102'),
(3, 'Engineering',             'Building B, Room 201'),
(4, 'Business Administration', 'Building C, Room 301'),
(5, 'Mathematics',             'Building A, Room 103');

-- ============================================================
--  INSTRUCTORS
--  Names and emails match the Instructor-role user accounts above.
-- ============================================================

INSERT IGNORE INTO instructors (instructor_id, first_name, last_name, email, department_id) VALUES
(1, 'Jose',      'Reyes',       'j.reyes@acadsystem.edu',       1),
(2, 'Maria',     'Santos',      'm.santos@acadsystem.edu',      2),
(3, 'Ana',       'Cruz',        'a.cruz@acadsystem.edu',        1),
(4, 'Roberto',   'Garcia',      'r.garcia@acadsystem.edu',      2),
(5, 'Lourdes',   'Bautista',    'l.bautista@acadsystem.edu',   3),
(6, 'Emmanuel',  'Dela Cruz',   'e.delacruz@acadsystem.edu',   4),
(7, 'Patricia',  'Villanueva',  'p.villanueva@acadsystem.edu', 5),
(8, 'Ramon',     'Flores',      'r.flores@acadsystem.edu',      2);

-- ============================================================
--  COURSES
-- ============================================================

INSERT IGNORE INTO courses (course_id, course_name, credits, department_id, instructor_id) VALUES
(1,  'Introduction to Programming',     3, 1, 1),
(2,  'Web Development Fundamentals',    3, 1, 3),
(3,  'Database Management Systems',     3, 2, 2),
(4,  'Data Structures and Algorithms',  4, 2, 4),
(5,  'Computer Networks',               3, 1, 1),
(6,  'Operating Systems',               3, 2, 8),
(7,  'Software Engineering',            3, 2, 2),
(8,  'Engineering Mathematics',         4, 3, 7),
(9,  'Principles of Management',        3, 4, 6),
(10, 'Business Communication',          3, 4, 6),
(11, 'Calculus I',                      4, 5, 7),
(12, 'Discrete Mathematics',            3, 5, 7),
(13, 'Capstone Project',                6, 1, 3),
(14, 'Human-Computer Interaction',      3, 1, 1),
(15, 'Mobile Application Development',  3, 2, 4);

-- ============================================================
--  STUDENTS
-- ============================================================

INSERT IGNORE INTO students (student_id, first_name, last_name, email, date_of_birth) VALUES
(1,  'Crisanto', 'Aquino',     'c.aquino@student.edu',     '2003-03-12'),
(2,  'Liza',     'Mendoza',    'l.mendoza@student.edu',    '2003-07-24'),
(3,  'Mark',     'Ramos',      'm.ramos@student.edu',      '2002-11-05'),
(4,  'Jessa',    'Torres',     'j.torres@student.edu',     '2003-01-18'),
(5,  'Brian',    'Castillo',   'b.castillo@student.edu',   '2002-09-30'),
(6,  'Nicole',   'Diaz',       'n.diaz@student.edu',       '2003-05-14'),
(7,  'Andrei',   'Reyes',      'a.reyes@student.edu',      '2002-12-22'),
(8,  'Samantha', 'Ocampo',     's.ocampo@student.edu',     '2003-08-09'),
(9,  'Gabriel',  'Pascual',    'g.pascual@student.edu',    '2002-04-17'),
(10, 'Hannah',   'Villanueva', 'h.villanueva@student.edu', '2003-02-28'),
(11, 'Jerome',   'Dela Rosa',  'j.delarosa@student.edu',   '2002-06-11'),
(12, 'Kristine', 'Bautista',   'k.bautista@student.edu',   '2003-10-03'),
(13, 'Patrick',  'Fernandez',  'p.fernandez@student.edu',  '2002-08-19'),
(14, 'Alyssa',   'Gutierrez',  'a.gutierrez@student.edu',  '2003-04-07'),
(15, 'Dominic',  'Hernandez',  'd.hernandez@student.edu',  '2002-01-25'),
(16, 'Carla',    'Ibarra',     'c.ibarra@student.edu',     '2003-09-16'),
(17, 'Francis',  'Javier',     'f.javier@student.edu',     '2002-03-08'),
(18, 'Maricel',  'Lacson',     'm.lacson@student.edu',     '2003-06-20'),
(19, 'Nathan',   'Magno',      'n.magno@student.edu',      '2002-07-13'),
(20, 'Sophia',   'Navarro',    's.navarro@student.edu',    '2003-11-29');

-- ============================================================
--  ENROLLMENTS
--  IDs 1-50:  graded
--  IDs 51-57: no grade (use for Grade Recording demo)
-- ============================================================

INSERT IGNORE INTO enrollments (enrollment_id, student_id, course_id, enrollment_date, grade) VALUES
(1,  1,  1,  '2024-01-08', 'A'),
(2,  2,  1,  '2024-01-08', 'B+'),
(3,  3,  1,  '2024-01-08', 'A-'),
(4,  4,  1,  '2024-01-08', 'B'),
(5,  5,  1,  '2024-01-08', 'A+'),
(6,  6,  2,  '2024-01-09', 'A'),
(7,  7,  2,  '2024-01-09', 'B+'),
(8,  8,  2,  '2024-01-09', 'A-'),
(9,  9,  2,  '2024-01-09', 'C+'),
(10, 10, 2,  '2024-01-09', 'B'),
(11, 1,  3,  '2024-01-10', 'B+'),
(12, 3,  3,  '2024-01-10', 'A'),
(13, 5,  3,  '2024-01-10', 'B'),
(14, 11, 3,  '2024-01-10', 'A-'),
(15, 12, 3,  '2024-01-10', 'B+'),
(16, 2,  4,  '2024-01-11', 'B'),
(17, 4,  4,  '2024-01-11', 'C+'),
(18, 7,  4,  '2024-01-11', 'B+'),
(19, 13, 4,  '2024-01-11', 'A-'),
(20, 14, 4,  '2024-01-11', 'B'),
(21, 8,  5,  '2024-01-12', 'A'),
(22, 9,  5,  '2024-01-12', 'B+'),
(23, 15, 5,  '2024-01-12', 'A-'),
(24, 16, 5,  '2024-01-12', 'B'),
(25, 10, 6,  '2024-01-13', 'B+'),
(26, 11, 6,  '2024-01-13', 'A'),
(27, 17, 6,  '2024-01-13', 'C+'),
(28, 18, 6,  '2024-01-13', 'B'),
(29, 6,  7,  '2024-01-14', 'A-'),
(30, 13, 7,  '2024-01-14', 'B+'),
(31, 19, 7,  '2024-01-14', 'A'),
(32, 20, 7,  '2024-01-14', 'B'),
(33, 3,  11, '2024-01-15', 'B'),
(34, 5,  11, '2024-01-15', 'A-'),
(35, 12, 11, '2024-01-15', 'C+'),
(36, 15, 11, '2024-01-15', 'B+'),
(37, 1,  12, '2024-01-16', 'B+'),
(38, 7,  12, '2024-01-16', 'A'),
(39, 14, 12, '2024-01-16', 'B'),
(40, 20, 12, '2024-01-16', 'A-'),
(41, 16, 9,  '2024-01-17', 'A'),
(42, 17, 9,  '2024-01-17', 'B+'),
(43, 18, 9,  '2024-01-17', 'A-'),
(44, 2,  13, '2024-01-18', 'A'),
(45, 4,  13, '2024-01-18', 'A-'),
(46, 9,  13, '2024-01-18', 'B+'),
(47, 6,  15, '2024-02-01', 'B+'),
(48, 8,  15, '2024-02-01', 'A'),
(49, 19, 15, '2024-02-01', 'A-'),
(50, 20, 15, '2024-02-01', 'B'),
(51, 11, 14, '2024-02-05', NULL),
(52, 12, 14, '2024-02-05', NULL),
(53, 13, 14, '2024-02-05', NULL),
(54, 16, 10, '2024-02-06', NULL),
(55, 17, 10, '2024-02-06', NULL),
(56, 15, 8,  '2024-02-07', NULL),
(57, 18, 8,  '2024-02-07', NULL);
