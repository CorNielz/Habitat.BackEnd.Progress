USE habitat_progress;

INSERT INTO roles (name, description)
VALUES
  ('USER', 'Usuário comum do aplicativo Habitat: Progress.'),
  ('ADMIN', 'Usuário administrador com acesso ao módulo administrativo básico.')
ON DUPLICATE KEY UPDATE description = VALUES(description);

INSERT INTO users (role_id, name, email, password_hash, is_active, created_at, updated_at, last_login_at)
SELECT r.id, 'Habitat Test User', 'test@local', '100000.AAECAwQFBgcICQoLDA0ODw==.OXIGQ0kq5id2vO+plZcThyZ2Mseaskr6zrwDi5cqTTA=', TRUE, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6), NULL
FROM roles r
WHERE r.name = 'USER'
  AND NOT EXISTS (SELECT 1 FROM users u WHERE u.email = 'test@local');

INSERT INTO users (role_id, name, email, password_hash, is_active, created_at, updated_at, last_login_at)
SELECT r.id, 'Habitat Admin User', 'admin@local', '100000.EBESExQVFhcYGRobHB0eHw==.J1Ky3Elx3xxnr0q/gYSJfIsv05zKeghcXN6LFveueto=', TRUE, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6), NULL
FROM roles r
WHERE r.name = 'ADMIN'
  AND NOT EXISTS (SELECT 1 FROM users u WHERE u.email = 'admin@local');

INSERT INTO user_settings (user_id, theme, default_dashboard_period, first_day_of_week, show_home_summary, updated_at)
SELECT u.id, 'SYSTEM', 'MONTH', 'MONDAY', TRUE, UTC_TIMESTAMP(6)
FROM users u
WHERE NOT EXISTS (SELECT 1 FROM user_settings s WHERE s.user_id = u.id);

INSERT INTO habits (user_id, title, description, frequency_type, frequency_value, start_date, is_active, created_at, updated_at)
SELECT u.id, 'Ler', 'Leitura diária', 'DAILY', '1', CURDATE(), TRUE, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)
FROM users u
WHERE u.email = 'test@local'
  AND NOT EXISTS (SELECT 1 FROM habits h WHERE h.user_id = u.id AND h.title = 'Ler');
