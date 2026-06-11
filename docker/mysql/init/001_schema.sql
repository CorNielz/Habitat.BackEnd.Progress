CREATE DATABASE IF NOT EXISTS habitat_progress
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

USE habitat_progress;

CREATE TABLE IF NOT EXISTS roles (
    id INT NOT NULL AUTO_INCREMENT,
    name ENUM('USER', 'ADMIN') NOT NULL,
    description VARCHAR(255) NULL,
    PRIMARY KEY (id),
    UNIQUE KEY uk_roles_name (name)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS users (
    id INT NOT NULL AUTO_INCREMENT,
    role_id INT NOT NULL,
    name VARCHAR(120) NOT NULL,
    email VARCHAR(180) NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at DATETIME(6) NOT NULL,
    updated_at DATETIME(6) NOT NULL,
    last_login_at DATETIME(6) NULL,
    PRIMARY KEY (id),
    UNIQUE KEY uk_users_email (email),
    KEY ix_users_role_id (role_id),
    CONSTRAINT fk_users_roles FOREIGN KEY (role_id) REFERENCES roles(id)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS user_settings (
    id INT NOT NULL AUTO_INCREMENT,
    user_id INT NOT NULL,
    theme ENUM('LIGHT', 'DARK', 'SYSTEM') NOT NULL DEFAULT 'SYSTEM',
    default_dashboard_period ENUM('WEEK', 'MONTH', 'YEAR') NOT NULL DEFAULT 'MONTH',
    first_day_of_week VARCHAR(20) NOT NULL DEFAULT 'MONDAY',
    show_home_summary BOOLEAN NOT NULL DEFAULT TRUE,
    updated_at DATETIME(6) NOT NULL,
    PRIMARY KEY (id),
    UNIQUE KEY uk_user_settings_user_id (user_id),
    CONSTRAINT fk_user_settings_users FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS habits (
    id INT NOT NULL AUTO_INCREMENT,
    user_id INT NOT NULL,
    title VARCHAR(120) NOT NULL,
    description TEXT NULL,
    frequency_type ENUM('DAILY', 'WEEKLY', 'MONTHLY', 'CUSTOM') NOT NULL,
    frequency_value VARCHAR(100) NOT NULL,
    start_date DATE NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at DATETIME(6) NOT NULL,
    updated_at DATETIME(6) NOT NULL,
    PRIMARY KEY (id),
    KEY ix_habits_user_id (user_id),
    KEY ix_habits_user_active (user_id, is_active),
    CONSTRAINT fk_habits_users FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS habit_records (
    id INT NOT NULL AUTO_INCREMENT,
    habit_id INT NOT NULL,
    record_date DATE NOT NULL,
    completed BOOLEAN NOT NULL DEFAULT TRUE,
    note TEXT NULL,
    recorded_at DATETIME(6) NOT NULL,
    PRIMARY KEY (id),
    UNIQUE KEY uk_habit_records_habit_date (habit_id, record_date),
    KEY ix_habit_records_date (record_date),
    CONSTRAINT fk_habit_records_habits FOREIGN KEY (habit_id) REFERENCES habits(id) ON DELETE CASCADE
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS notes (
    id INT NOT NULL AUTO_INCREMENT,
    user_id INT NOT NULL,
    title VARCHAR(150) NULL,
    content TEXT NOT NULL,
    note_date DATE NOT NULL,
    created_at DATETIME(6) NOT NULL,
    updated_at DATETIME(6) NOT NULL,
    PRIMARY KEY (id),
    KEY ix_notes_user_id (user_id),
    KEY ix_notes_user_date (user_id, note_date),
    CONSTRAINT fk_notes_users FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
) ENGINE=InnoDB;
