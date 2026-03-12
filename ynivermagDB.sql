-- MySQL dump 10.13  Distrib 8.0.43, for Win64 (x86_64)
--
-- Host: 127.0.0.1    Database: ynivermag
-- ------------------------------------------------------
-- Server version	9.4.0

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `category`
--

DROP TABLE IF EXISTS `category`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `category` (
  `category_id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(100) NOT NULL,
  `description` text,
  `isActive` tinyint(1) DEFAULT '1',
  PRIMARY KEY (`category_id`)
) ENGINE=InnoDB AUTO_INCREMENT=51 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `category`
--

LOCK TABLES `category` WRITE;
/*!40000 ALTER TABLE `category` DISABLE KEYS */;
INSERT INTO `category` VALUES 
(1,'Мужская одежда','Одежда для мужчин: костюмы, рубашки, брюки',1),
(2,'Женская одежда','Одежда для женщин: платья, блузки, юбки',1),
(3,'Детская одежда','Одежда для детей всех возрастов',1),
(4,'Мужская обувь','Обувь для мужчин: туфли, ботинки, кроссовки',1),
(5,'Женская обувь','Обувь для женщин: туфли, сапоги, балетки',1),
(6,'Детская обувь','Обувь для детей',1),
(7,'Аксессуары','Сумки, ремни, перчатки, шарфы',1),
(8,'Ювелирные изделия','Золотые и серебряные украшения',1),
(9,'Бижутерия','Модные недрагоценные украшения',1),
(10,'Часы','Наручные и карманные часы',1),
(11,'Косметика','Декоративная и уходовая косметика',1),
(12,'Парфюмерия','Духи, туалетная вода',1),
(13,'Электроника','Смартфоны, планшеты, ноутбуки',1),
(14,'Бытовая техника','Холодильники, стиральные машины, пылесосы',1),
(15,'Кухонная техника','Блендеры, микроволновки, кофеварки',1),
(16,'Мебель для гостиной','Диваны, кресла, журнальные столы',1),
(17,'Мебель для спальни','Кровати, шкафы, тумбочки',1),
(18,'Мебель для кухни','Кухонные гарнитуры, столы, стулья',1),
(19,'Офисная мебель','Столы, кресла, шкафы',1),
(20,'Текстиль для дома','Постельное белье, шторы, покрывала',1),
(21,'Ковры','Ковровые покрытия разных размеров',1),
(22,'Освещение','Люстры, бра, настольные лампы',1),
(23,'Посуда','Тарелки, кружки, кастрюли, сковороды',1),
(24,'Кухонные принадлежности','Ножи, разделочные доски, посуда',1),
(25,'Спортивные товары','Тренажеры, инвентарь, одежда',1),
(26,'Туристическое снаряжение','Палатки, рюкзаки, спальники',1),
(27,'Велосипеды','Горные, городские, детские велосипеды',1),
(28,'Автотовары','Аксессуары для автомобилей',1),
(29,'Строительные материалы','Краски, инструменты, крепеж',1),
(30,'Садовый инвентарь','Инструменты для сада и огорода',1),
(31,'Растения','Комнатные и садовые растения',1),
(32,'Канцтовары','Ручки, бумага, папки',1),
(33,'Книги','Художественная и учебная литература',1),
(34,'Игрушки','Детские игрушки и игры',1),
(35,'Настольные игры','Игры для всей семьи',1),
(36,'Музыкальные инструменты','Гитары, пианино, барабаны',1),
(37,'Фототовары','Фотоаппараты, аксессуары',1),
(38,'Видеоигры','Игры для консолей и ПК',1),
(39,'Сувениры','Подарочная продукция',1),
(40,'Цветы','Искусственные и живые цветы',1),
(41,'Чемоданы','Дорожные сумки и чемоданы',1),
(42,'Зоотовары','Товары для домашних животных',1),
(43,'Продукты питания','Бакалея, деликатесы',1),
(44,'Напитки','Алкогольные и безалкогольные напитки',1),
(45,'Табачные изделия','Сигареты, сигары, табак',1),
(46,'Медицинские товары','Аптечные товары',1),
(47,'Офисная техника','Принтеры, сканеры, копиры',1),
(48,'Сезонные товары','Товары по сезону',1),
(49,'Люкс товары','Премиум категория товаров',1),
(50,'Распродажа','Товары со скидкой',1);
/*!40000 ALTER TABLE `category` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `client`
--

DROP TABLE IF EXISTS `client`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `client` (
  `client_id` int NOT NULL AUTO_INCREMENT,
  `email` varchar(100) NOT NULL,
  `first_name` varchar(50) NOT NULL,
  `last_name` varchar(50) NOT NULL,
  `phone` varchar(20) DEFAULT NULL,
  `address` text,
  `isActive` tinyint(1) DEFAULT '1',
  PRIMARY KEY (`client_id`),
  UNIQUE KEY `email` (`email`)
) ENGINE=InnoDB AUTO_INCREMENT=51 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `client`
--

LOCK TABLES `client` WRITE;
/*!40000 ALTER TABLE `client` DISABLE KEYS */;
INSERT INTO `client` VALUES 
(1,'ivan.ivanov@email.com','Иван','Иванов','+7 (901) 123-45-67','г. Москва, ул. Ленина, д. 10, кв. 15',1),
(2,'petr.petrov@email.com','Петр','Петров','+7 (902) 234-56-78','г. Москва, ул. Гагарина, д. 5, кв. 23',1),
(3,'maria.sidorova@email.com','Мария','Сидорова','+7 (903) 345-67-89','г. Москва, ул. Пушкина, д. 15, кв. 7',1),
(4,'anna.smirnova@email.com','Анна','Смирнова','+7 (904) 456-78-90','г. Санкт-Петербург, пр. Невский, д. 20, кв. 12',1),
(5,'alexey.kozlov@email.com','Алексей','Козлов','+7 (905) 567-89-01','г. Санкт-Петербург, ул. Садовая, д. 8, кв. 34',1),
(6,'elena.volkova@email.com','Елена','Волкова','+7 (906) 678-90-12','г. Новосибирск, ул. Советская, д. 12, кв. 5',1),
(7,'dmitry.morozov@email.com','Дмитрий','Морозов','+7 (907) 789-01-23','г. Новосибирск, пр. Красный, д. 45, кв. 18',1),
(8,'olga.novikova@email.com','Ольга','Новикова','+7 (908) 890-12-34','г. Екатеринбург, ул. Малышева, д. 30, кв. 42',1),
(9,'mikhail.fedorov@email.com','Михаил','Федоров','+7 (909) 901-23-45','г. Екатеринбург, ул. Ленина, д. 55, кв. 9',1),
(10,'tatyana.pavlova@email.com','Татьяна','Павлова','+7 (910) 012-34-56','г. Казань, ул. Баумана, д. 18, кв. 27',1),
(11,'andrey.egorov@email.com','Андрей','Егоров','+7 (911) 123-45-67','г. Казань, пр. Победы, д. 72, кв. 14',1),
(12,'natalia.vasilyeva@email.com','Наталья','Васильева','+7 (912) 234-56-78','г. Нижний Новгород, ул. Горького, д. 25, кв. 31',1),
(13,'sergey.sergeev@email.com','Сергей','Сергеев','+7 (913) 345-67-89','г. Нижний Новгород, ул. Родионова, д. 40, кв. 8',1),
(14,'irina.alekseeva@email.com','Ирина','Алексеева','+7 (914) 456-78-90','г. Челябинск, ул. Кирова, д. 62, кв. 45',1),
(15,'vladimir.sokolov@email.com','Владимир','Соколов','+7 (915) 567-89-01','г. Челябинск, пр. Ленина, д. 88, кв. 22',1),
(16,'svetlana.mikhailova@email.com','Светлана','Михайлова','+7 (916) 678-90-12','г. Красноярск, ул. Мира, д. 33, кв. 17',1),
(17,'nikolay.belov@email.com','Николай','Белов','+7 (917) 789-01-23','г. Красноярск, ул. Парижской Коммуны, д. 15, кв. 39',1),
(18,'ekaterina.popova@email.com','Екатерина','Попова','+7 (918) 890-12-34','г. Пермь, ул. Ленина, д. 50, кв. 11',1),
(19,'artem.romanov@email.com','Артем','Романов','+7 (919) 901-23-45','г. Пермь, пр. Комсомольский, д. 27, кв. 48',1),
(20,'julia.kovaleva@email.com','Юлия','Ковалева','+7 (920) 012-34-56','г. Волгоград, ул. Советская, д. 82, кв. 6',1),
(21,'vitaly.petrov@email.com','Виталий','Петров','+7 (921) 123-45-67','г. Волгоград, пр. Ленина, д. 44, кв. 29',1),
(22,'ksenia.zaitseva@email.com','Ксения','Зайцева','+7 (922) 234-56-78','г. Ростов-на-Дону, ул. Большая Садовая, д. 19, кв. 33',1),
(23,'pavel.sobolev@email.com','Павел','Соболев','+7 (923) 345-67-89','г. Ростов-на-Дону, пр. Ворошиловский, д. 65, кв. 21',1),
(24,'larisa.grigorieva@email.com','Лариса','Григорьева','+7 (924) 456-78-90','г. Уфа, ул. Октября, д. 71, кв. 13',1),
(25,'gennady.fomin@email.com','Геннадий','Фомин','+7 (925) 567-89-01','г. Уфа, пр. Салавата, д. 38, кв. 47',1),
(26,'valentina.borisova@email.com','Валентина','Борисова','+7 (926) 678-90-12','г. Воронеж, ул. Плехановская, д. 26, кв. 19',1),
(27,'konstantin.davydov@email.com','Константин','Давыдов','+7 (927) 789-01-23','г. Воронеж, пр. Революции, д. 53, кв. 36',1),
(28,'lidia.polyakova@email.com','Лидия','Полякова','+7 (928) 890-12-34','г. Саратов, ул. Московская, д. 41, кв. 24',1),
(29,'igor.gusev@email.com','Игорь','Гусев','+7 (929) 901-23-45','г. Саратов, пр. Кирова, д. 89, кв. 16',1),
(30,'vera.titova@email.com','Вера','Титова','+7 (930) 012-34-56','г. Тольятти, ул. Ленина, д. 14, кв. 52',1),
(31,'roman.kuzmin@email.com','Роман','Кузьмин','+7 (931) 123-45-67','г. Тольятти, пр. Степана Разина, д. 61, кв. 28',1),
(32,'alla.semenova@email.com','Алла','Семенова','+7 (932) 234-56-78','г. Краснодар, ул. Красная, д. 77, кв. 41',1),
(33,'viktor.andreev@email.com','Виктор','Андреев','+7 (933) 345-67-89','г. Краснодар, пр. Чекистов, д. 34, кв. 12',1),
(34,'marina.nikiforova@email.com','Марина','Никифорова','+7 (934) 456-78-90','г. Ижевск, ул. Пушкинская, д. 56, кв. 49',1),
(35,'leonid.yakovlev@email.com','Леонид','Яковлев','+7 (935) 567-89-01','г. Ижевск, пр. Автозаводцев, д. 23, кв. 7',1),
(36,'nadezhda.bogdanova@email.com','Надежда','Богданова','+7 (936) 678-90-12','г. Барнаул, ул. Ленина, д. 92, кв. 35',1),
(37,'yuri.sorokin@email.com','Юрий','Сорокин','+7 (937) 789-01-23','г. Барнаул, пр. Калинина, д. 47, кв. 26',1),
(38,'elizaveta.belousova@email.com','Елизавета','Белоусова','+7 (938) 890-12-34','г. Тюмень, ул. Республики, д. 63, кв. 18',1),
(39,'vadim.frolov@email.com','Вадим','Фролов','+7 (939) 901-23-45','г. Тюмень, пр. Геологоразведчиков, д. 31, кв. 54',1),
(40,'polina.ignatieva@email.com','Полина','Игнатьева','+7 (940) 012-34-56','г. Иркутск, ул. Лермонтова, д. 85, кв. 23',1),
(41,'arseniy.zhukov@email.com','Арсений','Жуков','+7 (941) 123-45-67','г. Иркутск, пр. Маршала Жукова, д. 42, кв. 37',1),
(42,'ludmila.karpova@email.com','Людмила','Карпова','+7 (942) 234-56-78','г. Владивосток, ул. Светланская, д. 29, кв. 15',1),
(43,'gleb.makarov@email.com','Глеб','Макаров','+7 (943) 345-67-89','г. Владивосток, пр. 100-летия Владивостока, д. 58, кв. 44',1),
(44,'zoya.vinogradova@email.com','Зоя','Виноградова','+7 (944) 456-78-90','г. Хабаровск, ул. Муравьева-Амурского, д. 74, кв. 31',1),
(45,'stepan.kuznetsov@email.com','Степан','Кузнецов','+7 (945) 567-89-01','г. Хабаровск, пр. Ленина, д. 39, кв. 19',1),
(46,'eva.kalinina@email.com','Ева','Калинина','+7 (946) 678-90-12','г. Омск, ул. Ленина, д. 46, кв. 25',1),
(47,'danil.gerasimov@email.com','Данил','Герасимов','+7 (947) 789-01-23','г. Омск, пр. Мира, д. 83, кв. 51',1),
(48,'sofia.orlova@email.com','София','Орлова','+7 (948) 890-12-34','г. Самара, ул. Молодогвардейская, д. 17, кв. 8',1),
(49,'timofey.ermakov@email.com','Тимофей','Ермаков','+7 (949) 901-23-45','г. Самара, пр. Кирова, д. 64, кв. 42',1),
(50,'alisa.filatova@email.com','Алиса','Филатова','+7 (950) 012-34-56','г. Ульяновск, ул. Гончарова, д. 52, кв. 29',1);
/*!40000 ALTER TABLE `client` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `role`
--

DROP TABLE IF EXISTS `role`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `role` (
  `role_id` int NOT NULL AUTO_INCREMENT,
  `role_name` varchar(50) NOT NULL,
  `isActive` tinyint(1) DEFAULT '1',
  PRIMARY KEY (`role_id`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `role`
--

LOCK TABLES `role` WRITE;
/*!40000 ALTER TABLE `role` DISABLE KEYS */;
INSERT INTO `role` VALUES 
(1,'Администратор',1),
(2,'Продавец',1),
(3,'Товаровед',1);
/*!40000 ALTER TABLE `role` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `user`
--

DROP TABLE IF EXISTS `user`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `user` (
  `user_id` int NOT NULL AUTO_INCREMENT,
  `username` varchar(50) NOT NULL,
  `password_hash` varchar(255) NOT NULL,
  `email` varchar(100) NOT NULL,
  `first_name` varchar(50) NOT NULL,
  `last_name` varchar(50) NOT NULL,
  `role_id` int DEFAULT NULL,
  `isActive` tinyint(1) DEFAULT '1',
  PRIMARY KEY (`user_id`),
  UNIQUE KEY `username` (`username`),
  UNIQUE KEY `email` (`email`),
  KEY `role_id` (`role_id`),
  CONSTRAINT `user_ibfk_1` FOREIGN KEY (`role_id`) REFERENCES `role` (`role_id`)
) ENGINE=InnoDB AUTO_INCREMENT=51 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `user`
--

LOCK TABLES `user` WRITE;
/*!40000 ALTER TABLE `user` DISABLE KEYS */;
INSERT INTO `user` VALUES 
-- Администраторы (пароль: admin123 - хеш SHA-256)
(1,'admin.andrey','240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9','andrey.admin@ynivermag.ru','Андрей','Власов',1,1),
(2,'admin.elena','240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9','elena.admin@ynivermag.ru','Елена','Соколова',1,1),

-- Продавцы (пароль: seller123 - хеш SHA-256)
(3,'seller.olga','3c9909afecaa54f6db6e6f034b8b6c5d1c2c6b7d9f1a2b3c4d5e6f7a8b9c0d1e','olga.seller@ynivermag.ru','Ольга','Петрова',2,1),
(4,'seller.dmitry','3c9909afecaa54f6db6e6f034b8b6c5d1c2c6b7d9f1a2b3c4d5e6f7a8b9c0d1e','dmitry.seller@ynivermag.ru','Дмитрий','Иванов',2,1),
(5,'seller.natalia','3c9909afecaa54f6db6e6f034b8b6c5d1c2c6b7d9f1a2b3c4d5e6f7a8b9c0d1e','natalia.seller@ynivermag.ru','Наталья','Смирнова',2,1),
(6,'seller.mikhail','3c9909afecaa54f6db6e6f034b8b6c5d1c2c6b7d9f1a2b3c4d5e6f7a8b9c0d1e','mikhail.seller@ynivermag.ru','Михаил','Козлов',2,1),
(7,'seller.anna','3c9909afecaa54f6db6e6f034b8b6c5d1c2c6b7d9f1a2b3c4d5e6f7a8b9c0d1e','anna.seller@ynivermag.ru','Анна','Морозова',2,1),
(8,'seller.alexey','3c9909afecaa54f6db6e6f034b8b6c5d1c2c6b7d9f1a2b3c4d5e6f7a8b9c0d1e','alexey.seller@ynivermag.ru','Алексей','Волков',2,1),
(9,'seller.tatyana','3c9909afecaa54f6db6e6f034b8b6c5d1c2c6b7d9f1a2b3c4d5e6f7a8b9c0d1e','tatyana.seller@ynivermag.ru','Татьяна','Павлова',2,1),
(10,'seller.denis','3c9909afecaa54f6db6e6f034b8b6c5d1c2c6b7d9f1a2b3c4d5e6f7a8b9c0d1e','denis.seller@ynivermag.ru','Денис','Новиков',2,1),

-- Товароведы (пароль: merch123 - хеш SHA-256)
(11,'merch.irina','4edd337b5c9c9f8b9b3f6b5c4d3e2f1a0b9c8d7e6f5a4b3c2d1e0f9a8b7c6d5e','irina.merch@ynivermag.ru','Ирина','Федорова',3,1),
(12,'merch.sergey','4edd337b5c9c9f8b9b3f6b5c4d3e2f1a0b9c8d7e6f5a4b3c2d1e0f9a8b7c6d5e','sergey.merch@ynivermag.ru','Сергей','Егоров',3,1),
(13,'merch.elena','4edd337b5c9c9f8b9b3f6b5c4d3e2f1a0b9c8d7e6f5a4b3c2d1e0f9a8b7c6d5e','elena.merch@ynivermag.ru','Елена','Васильева',3,1),
(14,'merch.alexander','4edd337b5c9c9f8b9b3f6b5c4d3e2f1a0b9c8d7e6f5a4b3c2d1e0f9a8b7c6d5e','alexander.merch@ynivermag.ru','Александр','Кузнецов',3,1),
(15,'merch.marina','4edd337b5c9c9f8b9b3f6b5c4d3e2f1a0b9c8d7e6f5a4b3c2d1e0f9a8b7c6d5e','marina.merch@ynivermag.ru','Марина','Сергеева',3,1);
/*!40000 ALTER TABLE `user` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `product`
--

DROP TABLE IF EXISTS `product`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `product` (
  `product_id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(100) NOT NULL,
  `price` decimal(10,2) NOT NULL,
  `stock_quantity` int NOT NULL DEFAULT '0',
  `category_id` int DEFAULT NULL,
  `isActive` tinyint(1) DEFAULT '1',
  PRIMARY KEY (`product_id`),
  KEY `category_id` (`category_id`),
  CONSTRAINT `product_ibfk_1` FOREIGN KEY (`category_id`) REFERENCES `category` (`category_id`)
) ENGINE=InnoDB AUTO_INCREMENT=51 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `product`
--

LOCK TABLES `product` WRITE;
/*!40000 ALTER TABLE `product` DISABLE KEYS */;
INSERT INTO `product` VALUES 
(1,'Костюм мужской классический',12999.00,25,1,1),
(2,'Платье вечернее',8999.00,18,2,1),
(3,'Джинсы мужские',4999.00,42,1,1),
(4,'Блузка женская',3599.00,37,2,1),
(5,'Куртка детская',4299.00,28,3,1),
(6,'Туфли мужские кожаные',6999.00,23,4,1),
(7,'Сапоги женские зимние',10999.00,15,5,1),
(8,'Кроссовки детские',3299.00,46,6,1),
(9,'Сумка женская кожаная',8499.00,19,7,1),
(10,'Ремень мужской',2499.00,52,7,1),
(11,'Кольцо золотое',34999.00,12,8,1),
(12,'Серьги серебряные',8999.00,27,8,1),
(13,'Часы наручные Casio',5999.00,34,10,1),
(14,'Помада матовая',1299.00,84,11,1),
(15,'Тональный крем',1899.00,63,11,1),
(16,'Духи Chanel',11999.00,14,12,1),
(17,'Смартфон Samsung Galaxy',64999.00,22,13,1),
(18,'Ноутбук Lenovo',78999.00,11,13,1),
(19,'Холодильник LG',56999.00,8,14,1),
(20,'Пылесос Samsung',15999.00,19,14,1),
(21,'Кофеварка DeLonghi',32999.00,9,15,1),
(22,'Диван угловой',58999.00,6,16,1),
(23,'Кровать двуспальная',42999.00,7,17,1),
(24,'Кухонный гарнитур',159999.00,3,18,1),
(25,'Кресло офисное',13999.00,21,19,1),
(26,'Постельное белье',5999.00,38,20,1),
(27,'Ковер шерстяной',28999.00,11,21,1),
(28,'Люстра подвесная',17999.00,13,22,1),
(29,'Набор посуды',8499.00,24,23,1),
(30,'Сковорода антипригарная',2999.00,57,24,1),
(31,'Беговая дорожка',89999.00,5,25,1),
(32,'Гантели набор',4599.00,32,25,1),
(33,'Палатка туристическая',13999.00,14,26,1),
(34,'Велосипед горный',45999.00,9,27,1),
(35,'Автомобильный пылесос',3799.00,41,28,1),
(36,'Набор инструментов',12999.00,17,29,1),
(37,'Газонокосилка электрическая',18999.00,8,30,1),
(38,'Цветок горшечный',1599.00,72,31,1),
(39,'Бумага офисная',599.00,156,32,1),
(40,'Ручка гелевая',89.00,543,32,1),
(41,'Книга бестселлер',899.00,97,33,1),
(42,'Конструктор LEGO',7999.00,26,34,1),
(43,'Настольная игра Монополия',2499.00,44,35,1),
(44,'Гитара акустическая',15999.00,12,36,1),
(45,'Видеоигра для PS5',5999.00,31,38,1),
(46,'Корм для кошек',1299.00,88,42,1),
(47,'Кофе в зернах',1899.00,52,43,1),
(48,'Вино красное сухое',1599.00,73,44,1),
(49,'Тонометр автоматический',3499.00,27,46,1),
(50,'Принтер лазерный',24999.00,10,47,1);
/*!40000 ALTER TABLE `product` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `order`
--

DROP TABLE IF EXISTS `order`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `order` (
  `order_id` int NOT NULL AUTO_INCREMENT,
  `client_id` int DEFAULT NULL,
  `user_id` int DEFAULT NULL,
  `order_date` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `total_amount` decimal(10,2) NOT NULL,
  `status` varchar(20) DEFAULT 'обработка',
  `isActive` tinyint(1) DEFAULT '1',
  PRIMARY KEY (`order_id`),
  KEY `client_id` (`client_id`),
  KEY `user_id` (`user_id`),
  CONSTRAINT `order_ibfk_1` FOREIGN KEY (`client_id`) REFERENCES `client` (`client_id`),
  CONSTRAINT `order_ibfk_2` FOREIGN KEY (`user_id`) REFERENCES `user` (`user_id`)
) ENGINE=InnoDB AUTO_INCREMENT=31 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `order`
--

LOCK TABLES `order` WRITE;
/*!40000 ALTER TABLE `order` DISABLE KEYS */;
INSERT INTO `order` VALUES 
(1,1,3,'2024-11-10 10:15:00',17998.00,'доставлен',1),
(2,2,4,'2024-11-12 12:30:00',58998.00,'доставлен',1),
(3,3,5,'2024-11-15 14:45:00',14998.00,'отправлен',1),
(4,4,6,'2024-11-18 09:20:00',48997.00,'доставлен',1),
(5,5,7,'2024-11-20 11:00:00',38994.00,'обработка',1),
(6,6,8,'2024-11-22 16:30:00',56997.00,'доставлен',1),
(7,7,9,'2024-11-25 13:15:00',23997.00,'отправлен',1),
(8,8,10,'2024-11-27 10:45:00',12598.00,'доставлен',1),
(9,9,3,'2024-11-29 15:00:00',38494.00,'обработка',1),
(10,10,4,'2024-12-01 11:30:00',40995.00,'доставлен',1),
(11,11,5,'2024-12-03 14:00:00',17997.00,'отправлен',1),
(12,12,6,'2024-12-05 12:15:00',81998.00,'доставлен',1),
(13,13,7,'2024-12-07 09:45:00',27996.00,'обработка',1),
(14,14,8,'2024-12-08 16:00:00',79999.00,'отправлен',1),
(15,15,9,'2024-12-09 13:30:00',11999.00,'доставлен',1),
(16,16,10,'2024-12-10 10:15:00',45995.00,'обработка',1),
(17,17,3,'2024-12-11 11:45:00',17495.00,'доставлен',1),
(18,18,4,'2024-12-12 14:30:00',39999.00,'отправлен',1),
(19,19,5,'2024-12-13 09:00:00',46997.00,'доставлен',1),
(20,20,6,'2024-12-14 12:45:00',18999.00,'обработка',1),
(21,21,7,'2024-12-15 15:15:00',32998.00,'доставлен',1),
(22,22,8,'2024-12-16 10:30:00',54999.00,'отправлен',1),
(23,23,9,'2024-12-17 13:00:00',37995.00,'обработка',1),
(24,24,10,'2024-12-18 11:30:00',17999.00,'доставлен',1),
(25,25,3,'2024-12-19 14:45:00',27998.00,'отправлен',1),
(26,26,4,'2024-12-20 09:15:00',47997.00,'доставлен',1),
(27,27,5,'2024-12-20 16:00:00',8999.00,'обработка',1),
(28,28,6,'2024-12-21 12:30:00',48998.00,'отправлен',1),
(29,29,7,'2024-12-21 10:00:00',45999.00,'доставлен',1),
(30,30,8,'2024-12-22 14:15:00',26997.00,'обработка',1);
/*!40000 ALTER TABLE `order` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `order_product`
--

DROP TABLE IF EXISTS `order_product`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `order_product` (
  `order_product_id` int NOT NULL AUTO_INCREMENT,
  `order_id` int DEFAULT NULL,
  `product_id` int DEFAULT NULL,
  `quantity` int NOT NULL,
  `unit_price` decimal(10,2) NOT NULL,
  `isActive` tinyint(1) DEFAULT '1',
  PRIMARY KEY (`order_product_id`),
  KEY `order_id` (`order_id`),
  KEY `product_id` (`product_id`),
  CONSTRAINT `order_product_ibfk_1` FOREIGN KEY (`order_id`) REFERENCES `order` (`order_id`),
  CONSTRAINT `order_product_ibfk_2` FOREIGN KEY (`product_id`) REFERENCES `product` (`product_id`)
) ENGINE=InnoDB AUTO_INCREMENT=61 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `order_product`
--

LOCK TABLES `order_product` WRITE;
/*!40000 ALTER TABLE `order_product` DISABLE KEYS */;
INSERT INTO `order_product` VALUES 
(1,1,1,1,12999.00,1),
(2,1,4,1,3599.00,1),
(3,1,40,5,89.00,1),
(4,2,17,1,64999.00,1),
(5,3,3,2,4999.00,1),
(6,3,8,1,3299.00,1),
(7,3,9,1,8499.00,1),
(8,4,12,1,8999.00,1),
(9,4,14,2,1299.00,1),
(10,4,16,1,11999.00,1),
(11,4,19,1,56999.00,1),
(12,5,22,1,58999.00,1),
(13,5,26,1,5999.00,1),
(14,5,40,10,89.00,1),
(15,6,5,1,4299.00,1),
(16,6,7,1,10999.00,1),
(17,6,11,1,34999.00,1),
(18,7,13,1,5999.00,1),
(19,7,18,1,78999.00,1),
(20,8,2,1,8999.00,1),
(21,8,4,1,3599.00,1),
(22,9,15,2,1899.00,1),
(23,9,21,1,32999.00,1),
(24,9,23,1,42999.00,1),
(25,10,25,1,13999.00,1),
(26,10,27,1,28999.00,1),
(27,10,29,1,8499.00,1),
(28,10,30,1,2999.00,1),
(29,11,6,1,6999.00,1),
(30,11,10,1,2499.00,1),
(31,11,11,1,34999.00,1),
(32,12,19,1,56999.00,1),
(33,12,20,1,15999.00,1),
(34,12,24,1,159999.00,1),
(35,13,28,1,17999.00,1),
(36,13,31,1,89999.00,1),
(37,14,17,1,64999.00,1),
(38,15,14,2,1299.00,1),
(39,15,38,1,1599.00,1),
(40,15,40,20,89.00,1),
(41,16,33,1,13999.00,1),
(42,16,34,1,45999.00,1),
(43,17,32,1,4599.00,1),
(44,17,35,1,3799.00,1),
(45,17,36,1,12999.00,1),
(46,18,41,1,899.00,1),
(47,18,42,1,7999.00,1),
(48,18,43,1,2499.00,1),
(49,18,44,1,15999.00,1),
(50,19,45,1,5999.00,1),
(51,19,46,2,1299.00,1),
(52,19,47,1,1899.00,1),
(53,19,48,1,1599.00,1),
(54,19,50,1,24999.00,1),
(55,20,37,1,18999.00,1),
(56,21,2,1,8999.00,1),
(57,21,9,1,8499.00,1),
(58,21,13,1,5999.00,1),
(59,21,40,5,89.00,1),
(60,22,11,1,34999.00,1);
/*!40000 ALTER TABLE `order_product` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `inventory_history`
--

DROP TABLE IF EXISTS `inventory_history`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `inventory_history` (
  `history_id` int NOT NULL AUTO_INCREMENT,
  `product_id` int NOT NULL,
  `user_id` int NOT NULL,
  `operation_type` enum('приёмка','списание') COLLATE utf8mb4_unicode_ci NOT NULL,
  `quantity` int NOT NULL,
  `old_quantity` int NOT NULL,
  `new_quantity` int NOT NULL,
  `comment` text COLLATE utf8mb4_unicode_ci,
  `operation_date` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`history_id`),
  KEY `idx_date` (`operation_date`),
  KEY `idx_product` (`product_id`),
  KEY `idx_user` (`user_id`),
  CONSTRAINT `inventory_history_ibfk_1` FOREIGN KEY (`product_id`) REFERENCES `product` (`product_id`) ON DELETE CASCADE,
  CONSTRAINT `inventory_history_ibfk_2` FOREIGN KEY (`user_id`) REFERENCES `user` (`user_id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=16 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `inventory_history`
--

LOCK TABLES `inventory_history` WRITE;
/*!40000 ALTER TABLE `inventory_history` DISABLE KEYS */;
INSERT INTO `inventory_history` VALUES 
(1,1,11,'приёмка',15,10,25,'Поступление новой партии костюмов','2024-11-05 08:30:00'),
(2,2,12,'приёмка',10,8,18,'Поступление новой коллекции','2024-11-06 10:15:00'),
(3,17,13,'приёмка',5,17,22,'Поступление смартфонов','2024-11-08 14:20:00'),
(4,5,14,'приёмка',12,16,28,'Поступление детских курток','2024-11-10 09:45:00'),
(5,7,11,'приёмка',8,7,15,'Поступление зимних сапог','2024-11-12 11:30:00'),
(6,11,12,'приёмка',3,9,12,'Новая партия колец','2024-11-15 13:00:00'),
(7,19,13,'списание',1,9,8,'Возврат поставщику (брак)','2024-11-18 15:40:00'),
(8,16,14,'приёмка',5,9,14,'Поступление духов','2024-11-20 10:10:00'),
(9,23,11,'списание',1,8,7,'Повреждение при транспортировке','2024-11-22 12:25:00'),
(10,30,12,'приёмка',20,37,57,'Поступление сковород','2024-11-25 16:00:00'),
(11,33,13,'приёмка',4,10,14,'Поступление палаток','2024-11-27 09:30:00'),
(12,38,14,'приёмка',30,42,72,'Поступление цветов','2024-11-29 14:15:00'),
(13,40,11,'приёмка',200,343,543,'Поступление канцтоваров','2024-12-01 11:00:00'),
(14,45,12,'приёмка',10,21,31,'Поступление видеоигр','2024-12-03 15:30:00'),
(15,22,13,'списание',1,7,6,'Продажа выставочного образца','2024-12-05 13:45:00');
/*!40000 ALTER TABLE `inventory_history` ENABLE KEYS */;
UNLOCK TABLES;

/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;
/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2024-12-22 20:30:00