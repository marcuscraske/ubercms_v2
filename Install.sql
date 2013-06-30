-- Drop tables
SET FOREIGN_KEY_CHECKS=0;
DROP TABLE IF EXISTS `cms_plugins`;
DROP TABLE IF EXISTS `cms_settings`;
DROP TABLE IF EXISTS `cms_urlrewriting`;
DROP TABLE IF EXISTS `cms_templates`;
DROP TABLE IF EXISTS `cms_email_queue`;
DROP TABLE IF EXISTS `cms_plugin_handlers`;
SET FOREIGN_KEY_CHECKS=1;

-- Create tables
CREATE TABLE IF NOT EXISTS cms_plugins
(
	pluginid INT PRIMARY KEY AUTO_INCREMENT,
	title TEXT,
	directory TEXT,
	classpath TEXT,
	cycle_interval INT DEFAULT 0,
	priority INT DEFAULT 0,
	state INT DEFAULT 0
);
CREATE TABLE IF NOT EXISTS cms_plugin_handlers
(
	pluginid INT PRIMARY KEY AUTO_INCREMENT,
	request_start VARCHAR(1) DEFAULT 0,
	request_end VARCHAR(1) DEFAULT 0,
	page_error VARCHAR(1) DEFAULT 0,
	page_not_found VARCHAR(1) DEFAULT 0,
	cms_start VARCHAR(1) DEFAULT 0,
	cms_end VARCHAR(1) DEFAULT 0,
	cycle_interval INT DEFAULT 0
);
CREATE TABLE IF NOT EXISTS cms_settings
(
	path VARCHAR(128) PRIMARY KEY,
	pluginid INT,
	FOREIGN KEY(`pluginid`) REFERENCES `cms_plugins`(`pluginid`) ON UPDATE CASCADE ON DELETE CASCADE,
	value TEXT,
	description TEXT
);
CREATE TABLE IF NOT EXISTS cms_urlrewriting
(
	urlid INT PRIMARY KEY AUTO_INCREMENT,
	parent INT,
	FOREIGN KEY(`parent`) REFERENCES `cms_urlrewriting`(`urlid`) ON UPDATE CASCADE ON DELETE CASCADE,
	pluginid INT,
	FOREIGN KEY(`pluginid`) REFERENCES `cms_plugins`(`pluginid`) ON UPDATE CASCADE ON DELETE CASCADE,
	full_path TEXT,
	priority INT DEFAULT 0
);
CREATE TABLE IF NOT EXISTS cms_templates
(
	path VARCHAR(128) PRIMARY KEY,
	pluginid INT,
	FOREIGN KEY(`pluginid`) REFERENCES `cms_plugins`(`pluginid`) ON UPDATE CASCADE ON DELETE CASCADE,
	description TEXT,
	html TEXT
);
CREATE TABLE IF NOT EXISTS cms_template_handlers
(
	path VARCHAR(128) PRIMARY KEY,
	pluginid INT,
	FOREIGN KEY(`pluginid`) REFERENCES `cms_plugins`(`pluginid`) ON UPDATE CASCADE ON DELETE CASCADE,
	classpath TEXT,
	function_name TEXT
);
CREATE TABLE IF NOT EXISTS cms_email_queue
(
	emailid INT PRIMARY KEY AUTO_INCREMENT,
	email TEXT,
	subject TEXT,
	body TEXT,
	html VARCHAR(1) DEFAULT 1
);

-- Insert core settings
INSERT INTO cms_settings (path, pluginid, value, description) VALUES('core/default_handler', NULL, 'home', 'The default module-handler for the home-page/an empty request path.');
