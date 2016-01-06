DROP TABLE IF EXISTS `task`;
CREATE TABLE IF NOT EXISTS `task` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `uid` varchar(64) NOT NULL,
  `user_id` int(11) unsigned NOT NULL,
  `access_token` varchar(128) NOT NULL,
  `time_created` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `responsed` tinyint(1) NOT NULL DEFAULT '0',
  `response` mediumtext,
  `time_up` datetime DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `uid` (`uid`),
  KEY `responsed` (`responsed`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 ;
