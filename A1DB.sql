-- MariaDB dump 10.19  Distrib 10.5.18-MariaDB, for Linux (x86_64)
--
-- Host: localhost    Database: FKData
-- ------------------------------------------------------
-- Server version	10.5.18-MariaDB

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `mp_3`
--

DROP TABLE IF EXISTS `mp_3`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mp_3` (
  `username` varchar(50) DEFAULT NULL,
  `userID` int(11) DEFAULT NULL,
  `connectionID` varchar(50) DEFAULT NULL,
  `challenge` int(11) DEFAULT NULL,
  `challenger` int(11) DEFAULT NULL,
  `challengerInfo` varchar(10) DEFAULT NULL,
  `playerInfo` varchar(10) DEFAULT NULL,
  `score` int(11) DEFAULT NULL,
  `ready` int(11) DEFAULT NULL,
  `health` int(11) DEFAULT NULL,
  `lives` int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COLLATE=latin1_swedish_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mp_3`
--

LOCK TABLES `mp_3` WRITE;
/*!40000 ALTER TABLE `mp_3` DISABLE KEYS */;
/*!40000 ALTER TABLE `mp_3` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mp_4`
--

DROP TABLE IF EXISTS `mp_4`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mp_4` (
  `username` varchar(50) DEFAULT NULL,
  `userID` int(11) DEFAULT NULL,
  `connectionID` varchar(50) DEFAULT NULL,
  `challenge` int(11) DEFAULT NULL,
  `challenger` int(11) DEFAULT NULL,
  `challengerInfo` varchar(10) DEFAULT NULL,
  `playerInfo` varchar(10) DEFAULT NULL,
  `score` int(11) DEFAULT NULL,
  `ready` int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COLLATE=latin1_swedish_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mp_4`
--

LOCK TABLES `mp_4` WRITE;
/*!40000 ALTER TABLE `mp_4` DISABLE KEYS */;
/*!40000 ALTER TABLE `mp_4` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mp_5`
--

DROP TABLE IF EXISTS `mp_5`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mp_5` (
  `username` varchar(50) DEFAULT NULL,
  `userID` int(11) DEFAULT NULL,
  `connectionID` varchar(50) DEFAULT NULL,
  `challenge` int(11) DEFAULT NULL,
  `challenger` int(11) DEFAULT NULL,
  `challengerInfo` varchar(10) DEFAULT NULL,
  `playerInfo` varchar(10) DEFAULT NULL,
  `score` int(11) DEFAULT NULL,
  `ready` int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COLLATE=latin1_swedish_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mp_5`
--

LOCK TABLES `mp_5` WRITE;
/*!40000 ALTER TABLE `mp_5` DISABLE KEYS */;
/*!40000 ALTER TABLE `mp_5` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mp_6`
--

DROP TABLE IF EXISTS `mp_6`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mp_6` (
  `username` varchar(50) DEFAULT NULL,
  `userID` int(11) DEFAULT NULL,
  `connectionID` varchar(50) DEFAULT NULL,
  `challenge` int(11) DEFAULT NULL,
  `challenger` int(11) DEFAULT NULL,
  `challengerInfo` varchar(10) DEFAULT NULL,
  `playerInfo` varchar(10) DEFAULT NULL,
  `score` int(11) DEFAULT NULL,
  `ready` int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COLLATE=latin1_swedish_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mp_6`
--

LOCK TABLES `mp_6` WRITE;
/*!40000 ALTER TABLE `mp_6` DISABLE KEYS */;
/*!40000 ALTER TABLE `mp_6` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `t_cleaning`
--

DROP TABLE IF EXISTS `t_cleaning`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `t_cleaning` (
  `id` int(11) NOT NULL,
  `cost` int(11) DEFAULT NULL,
  `rid` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COLLATE=latin1_swedish_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `t_cleaning`
--

LOCK TABLES `t_cleaning` WRITE;
/*!40000 ALTER TABLE `t_cleaning` DISABLE KEYS */;
INSERT INTO `t_cleaning` VALUES (0,100,'70021a');
/*!40000 ALTER TABLE `t_cleaning` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `t_familiar`
--

DROP TABLE IF EXISTS `t_familiar`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `t_familiar` (
  `id` int(11) NOT NULL,
  `cost` int(11) DEFAULT NULL,
  `discountedCost` int(11) DEFAULT NULL,
  `duration` int(11) DEFAULT NULL,
  `rid` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COLLATE=latin1_swedish_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `t_familiar`
--

LOCK TABLES `t_familiar` WRITE;
/*!40000 ALTER TABLE `t_familiar` DISABLE KEYS */;
INSERT INTO `t_familiar` VALUES (0,100,50,720,'80036a'),(1,100,50,720,'80035a'),(2,100,50,720,'80034a'),(3,100,50,720,'80033a'),(4,100,50,720,'80032a'),(5,100,50,720,'80031a'),(6,100,50,720,'80030a'),(7,100,50,720,'80029a'),(8,100,50,720,'80028a'),(9,100,50,720,'80027a'),(10,100,50,720,'80026a'),(11,100,50,720,'80025a'),(12,100,50,720,'80017a'),(13,100,50,720,'80016a'),(14,100,50,720,'80015a'),(15,100,50,720,'80007a'),(16,100,50,720,'80006a'),(17,100,50,720,'80005a'),(18,100,50,720,'80004a'),(19,100,50,720,'80003a'),(20,100,50,720,'80002a'),(21,100,50,720,'80001a'),(22,100,50,720,'80000a');
/*!40000 ALTER TABLE `t_familiar` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `t_items`
--

DROP TABLE IF EXISTS `t_items`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `t_items` (
  `id` int(11) NOT NULL,
  `cost` int(255) DEFAULT NULL,
  `rid` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COLLATE=latin1_swedish_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `t_items`
--

LOCK TABLES `t_items` WRITE;
/*!40000 ALTER TABLE `t_items` DISABLE KEYS */;
INSERT INTO `t_items` VALUES (0,100,'70001a'),(1,100,'70002a'),(2,100,'70003a'),(3,100,'70003b'),(4,100,'70003c'),(5,100,'70003d'),(6,100,'70003e'),(7,100,'70004a'),(8,100,'70005a'),(9,100,'70006a'),(10,100,'70006b'),(11,100,'70007a'),(12,100,'70007b'),(13,100,'70007c'),(14,100,'70007d'),(15,100,'70008a'),(16,100,'70008b'),(17,100,'70008c'),(18,100,'70008d'),(19,100,'70009a'),(20,100,'70010a'),(21,100,'70011a'),(22,100,'70012a'),(23,100,'70012b'),(24,100,'70012c'),(25,100,'70013a'),(26,100,'70014a'),(27,100,'70015a'),(28,100,'70016a'),(29,100,'70017a'),(30,100,'70018a'),(31,100,'70019a'),(32,100,'70020a'),(33,100,'70022a'),(34,100,'70022b'),(35,100,'70022c'),(36,100,'70023a'),(37,100,'70024a'),(38,100,'70025a'),(39,100,'70025b'),(40,100,'70025c'),(41,100,'70026a'),(42,100,'70100a'),(43,100,'70100b'),(44,100,'70100c'),(45,100,'70100d'),(46,100,'70100e'),(47,100,'70100f'),(48,100,'70100g'),(49,100,'70101a'),(50,100,'70101b'),(51,100,'70101c'),(52,100,'70101d'),(53,100,'70101e'),(54,100,'70101f'),(55,100,'70101g'),(56,100,'70102a'),(57,100,'70102b'),(58,100,'70102c'),(59,100,'70102d'),(60,100,'70102e'),(61,100,'70102f'),(62,100,'70102g'),(63,100,'70103a'),(64,100,'70103b'),(65,100,'70103c'),(66,100,'70103d'),(67,100,'70103e'),(68,100,'70103f'),(69,100,'70103g'),(70,100,'70104a'),(71,100,'70104b'),(72,100,'70104c'),(73,100,'70104d'),(74,100,'70104e'),(75,100,'70104f'),(76,100,'70104g'),(77,100,'70105a'),(78,100,'70105b'),(79,100,'70105c'),(80,100,'70105d'),(81,100,'70105e'),(82,100,'70105f'),(83,100,'70105g'),(84,100,'70106a'),(85,100,'70106b'),(86,100,'70106c'),(87,100,'70106d'),(88,100,'70106e'),(89,100,'70106f'),(90,100,'70106g'),(91,100,'70107a'),(92,100,'70107b'),(93,100,'70107c'),(94,100,'70107d'),(95,100,'70107e'),(96,100,'70107f'),(97,100,'70107g'),(98,100,'70108a'),(99,100,'70108b'),(100,100,'70108c'),(101,100,'70108d'),(102,100,'70108e'),(103,100,'70108f'),(104,100,'70108g'),(105,100,'70109a'),(106,100,'70109b'),(107,100,'70109c'),(108,100,'70109d'),(109,100,'70109e'),(110,100,'70109f'),(111,100,'70109g'),(112,100,'70110a'),(113,100,'70110b'),(114,100,'70110c'),(115,100,'70110d'),(116,100,'70110e'),(117,100,'70110f'),(118,100,'70110g'),(119,100,'70111a'),(120,100,'70111b'),(121,100,'70111c'),(122,100,'70111d'),(123,100,'70111e'),(124,100,'70111f'),(125,100,'70111g'),(126,100,'70112a'),(127,100,'70112b'),(128,100,'70112c'),(129,100,'70112d'),(130,100,'70112e'),(131,100,'70112f'),(132,100,'70112g'),(133,100,'70113a'),(134,100,'70113b'),(135,100,'70113c'),(136,100,'70113d'),(137,100,'70113e'),(138,100,'70113f'),(139,100,'70113g'),(140,100,'70114a'),(141,100,'70114b'),(142,100,'70114c'),(143,100,'70114d'),(144,100,'70114e'),(145,100,'70114f'),(146,100,'70114g'),(147,100,'70115a'),(148,100,'70115b'),(149,100,'70115c'),(150,100,'70115d'),(151,100,'70115e'),(152,100,'70115f'),(153,100,'70115g'),(154,100,'70116a'),(155,100,'70116b'),(156,100,'70116c'),(157,100,'70116d'),(158,100,'70116e'),(159,100,'70116f'),(160,100,'70116g'),(161,100,'70117a'),(162,100,'70117b'),(163,100,'70117c'),(164,100,'70117d'),(165,100,'70117e'),(166,100,'70117f'),(167,100,'70117g'),(168,100,'70118a'),(169,100,'70118b'),(170,100,'70118c'),(171,100,'70118d'),(172,100,'70118e'),(173,100,'70118f'),(174,100,'70118g'),(175,100,'70119a'),(176,100,'70119b'),(177,100,'70119c'),(178,100,'70119d'),(179,100,'70119e'),(180,100,'70119f'),(181,100,'70119g'),(182,100,'70120a'),(183,100,'70120b'),(184,100,'70120c'),(185,100,'70120d'),(186,100,'70120e'),(187,100,'70120f'),(188,100,'70120g'),(189,100,'70121a'),(190,100,'70121b'),(191,100,'70121c'),(192,100,'70121d'),(193,100,'70121e'),(194,100,'70121f'),(195,100,'70121g'),(196,100,'70122a'),(197,100,'70122b'),(198,100,'70122c'),(199,100,'70122d'),(200,100,'70122e'),(201,100,'70122f'),(202,100,'70122g'),(203,100,'70123a'),(204,100,'70123b'),(205,100,'70123c'),(206,100,'70123d'),(207,100,'70123e'),(208,100,'70123f'),(209,100,'70123g'),(210,100,'70124a'),(211,100,'70124b'),(212,100,'70124c'),(213,100,'70124d'),(214,100,'70124e'),(215,100,'70124f'),(216,100,'70124g'),(217,100,'70125a'),(218,100,'70125b'),(219,100,'70125c'),(220,100,'70125d'),(221,100,'70125e'),(222,100,'70125f'),(223,100,'70125g'),(224,100,'70126a'),(225,100,'70126b'),(226,100,'70126c'),(227,100,'70126d'),(228,100,'70126e'),(229,100,'70126f'),(230,100,'70126g'),(231,100,'70127a'),(232,100,'70127b'),(233,100,'70127c'),(234,100,'70127d'),(235,100,'70127e'),(236,100,'70127f'),(237,100,'70127g'),(238,100,'70128a'),(239,100,'70128b'),(240,100,'70128c'),(241,100,'70128d'),(242,100,'70128e'),(243,100,'70128f'),(244,100,'70128g'),(245,100,'70129a'),(246,100,'70129b'),(247,100,'70129c'),(248,100,'70129d'),(249,100,'70129e'),(250,100,'70129f'),(251,100,'70129g'),(252,100,'70130a'),(253,100,'70130b'),(254,100,'70130c'),(255,100,'70130d'),(256,100,'70130e'),(257,100,'70130f'),(258,100,'70130g'),(259,100,'70131a'),(260,100,'70131b'),(261,100,'70131c'),(262,100,'70131d'),(263,100,'70131e'),(264,100,'70131f'),(265,100,'70131g'),(266,100,'70132a'),(267,100,'70132b'),(268,100,'70132c'),(269,100,'70132d'),(270,100,'70132e'),(271,100,'70132f'),(272,100,'70132g'),(273,100,'70133a'),(274,100,'70133b'),(275,100,'70133c'),(276,100,'70133d'),(277,100,'70133e'),(278,100,'70133f'),(279,100,'70133g'),(280,100,'70134a'),(281,100,'70134b'),(282,100,'70134c'),(283,100,'70134d'),(284,100,'70134e'),(285,100,'70134f'),(286,100,'70134g'),(287,100,'70135a'),(288,100,'70135b'),(289,100,'70135c'),(290,100,'70135d'),(291,100,'70135e'),(292,100,'70135f'),(293,100,'70135g'),(294,100,'70136a'),(295,100,'70136b'),(296,100,'70136c'),(297,100,'70136d'),(298,100,'70136e'),(299,100,'70136f'),(300,100,'70136g'),(301,100,'70137a'),(302,100,'70137b'),(303,100,'70137c'),(304,100,'70137d'),(305,100,'70137e'),(306,100,'70137f'),(307,100,'70137g'),(308,100,'70138a'),(309,100,'70138b'),(310,100,'70138c'),(311,100,'70138d'),(312,100,'70138e'),(313,100,'70138f'),(314,100,'70138g'),(315,100,'70139a'),(316,100,'70139b'),(317,100,'70139c'),(318,100,'70139d'),(319,100,'70139e'),(320,100,'70139f'),(321,100,'70139g'),(322,100,'70140a'),(323,100,'70140b'),(324,100,'70140c'),(325,100,'70140d'),(326,100,'70140e'),(327,100,'70140f'),(328,100,'70140g'),(329,100,'70141a'),(330,100,'70141b'),(331,100,'70141c'),(332,100,'70141d'),(333,100,'70141e'),(334,100,'70141f'),(335,100,'70141g'),(336,100,'70142a'),(337,100,'70142b'),(338,100,'70142c'),(339,100,'70142d'),(340,100,'70142e'),(341,100,'70142f'),(342,100,'70142g'),(343,100,'70143a'),(344,100,'70143b'),(345,100,'70143c'),(346,100,'70143d'),(347,100,'70143e'),(348,100,'70143f'),(349,100,'70143g'),(350,100,'70144a'),(351,100,'70144b'),(352,100,'70144c'),(353,100,'70144d'),(354,100,'70144e'),(355,100,'70144f'),(356,100,'70144g'),(357,100,'70145a'),(358,100,'70145b'),(359,100,'70145c'),(360,100,'70145d'),(361,100,'70145e'),(362,100,'70145f'),(363,100,'70145g'),(364,100,'70146a'),(365,100,'70146b'),(366,100,'70146c'),(367,100,'70146d'),(368,100,'70146e'),(369,100,'70146f'),(370,100,'70146g'),(371,100,'70147a'),(372,100,'70147b'),(373,100,'70147c'),(374,100,'70147d'),(375,100,'70147e'),(376,100,'70147f'),(377,100,'70147g'),(378,100,'70148a'),(379,100,'70148b'),(380,100,'70148c'),(381,100,'70148d'),(382,100,'70148e'),(383,100,'70148f'),(384,100,'70148g'),(385,100,'70149a'),(386,100,'70149b'),(387,100,'70149c'),(388,100,'70149d'),(389,100,'70149e'),(390,100,'70149f'),(391,100,'70149g'),(392,100,'70150a'),(393,100,'70150b'),(394,100,'70150c'),(395,100,'70150d'),(396,100,'70150e'),(397,100,'70150f'),(398,100,'70150g'),(399,100,'70151a'),(400,100,'70151b'),(401,100,'70151c'),(402,100,'70151d'),(403,100,'70151e'),(404,100,'70151f'),(405,100,'70151g'),(406,100,'70152a'),(407,100,'70152b'),(408,100,'70152c'),(409,100,'70152d'),(410,100,'70152e'),(411,100,'70152f'),(412,100,'70152g'),(413,100,'70153a'),(414,100,'70153b'),(415,100,'70153c'),(416,100,'70153d'),(417,100,'70153e'),(418,100,'70153f'),(419,100,'70153g'),(420,100,'70154a'),(421,100,'70154b'),(422,100,'70154c'),(423,100,'70154d'),(424,100,'70154e'),(425,100,'70154f'),(426,100,'70154g'),(427,100,'70155a'),(428,100,'70155b'),(429,100,'70155c'),(430,100,'70155d'),(431,100,'70155e'),(432,100,'70155f'),(433,100,'70155g'),(434,100,'70156a'),(435,100,'70156b'),(436,100,'70156c'),(437,100,'70156d'),(438,100,'70156e'),(439,100,'70156f'),(440,100,'70156g'),(441,100,'70157a'),(442,100,'70157b'),(443,100,'70157c'),(444,100,'70157d'),(445,100,'70157e'),(446,100,'70157f'),(447,100,'70157g'),(448,100,'70158a'),(449,100,'70158b'),(450,100,'70158c'),(451,100,'70158d'),(452,100,'70158e'),(453,100,'70158f'),(454,100,'70158g'),(455,100,'70159a'),(456,100,'70159b'),(457,100,'70159c'),(458,100,'70159d'),(459,100,'70159e'),(460,100,'70159f'),(461,100,'70159g'),(462,100,'70160a'),(463,100,'70160b'),(464,100,'70160c'),(465,100,'70160d'),(466,100,'70160e'),(467,100,'70160f'),(468,100,'70160g'),(469,100,'70161a'),(470,100,'70161b'),(471,100,'70161c'),(472,100,'70161d'),(473,100,'70161e'),(474,100,'70161f'),(475,100,'70161g'),(476,100,'70162a'),(477,100,'70162b'),(478,100,'70162c'),(479,100,'70163a'),(480,100,'70163b'),(481,100,'70163c'),(482,100,'70163d'),(483,100,'70164a'),(484,100,'70164b'),(485,100,'70165a'),(486,100,'70165b'),(487,100,'70165c'),(488,100,'70167a'),(489,100,'70166a'),(490,100,'70168a');
/*!40000 ALTER TABLE `t_items` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `t_jammer`
--

DROP TABLE IF EXISTS `t_jammer`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `t_jammer` (
  `id` int(11) NOT NULL,
  `cost` int(11) DEFAULT NULL,
  `quantity` int(11) DEFAULT NULL,
  `rid` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COLLATE=latin1_swedish_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `t_jammer`
--

LOCK TABLES `t_jammer` WRITE;
/*!40000 ALTER TABLE `t_jammer` DISABLE KEYS */;
INSERT INTO `t_jammer` VALUES (0,100,1,'80014a'),(1,100,5,'80014a'),(2,100,10,'80014a'),(3,100,25,'80014a'),(4,100,50,'80014a'),(5,100,100,'80014a');
/*!40000 ALTER TABLE `t_jammer` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `t_mood`
--

DROP TABLE IF EXISTS `t_mood`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `t_mood` (
  `id` int(11) NOT NULL,
  `cost` int(11) DEFAULT NULL,
  `rid` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COLLATE=latin1_swedish_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `t_mood`
--

LOCK TABLES `t_mood` WRITE;
/*!40000 ALTER TABLE `t_mood` DISABLE KEYS */;
INSERT INTO `t_mood` VALUES (0,100,'80041a'),(1,100,'80040a'),(2,100,'80039a'),(3,100,'80038a'),(4,100,'80037a'),(5,100,'80024a'),(6,100,'80023a'),(7,100,'80022a'),(8,100,'80021a'),(9,100,'80020a'),(10,100,'80019a'),(11,100,'80018a'),(12,100,'80013a'),(13,100,'80012a'),(14,100,'80011a'),(15,100,'80010a'),(16,100,'80009a'),(17,100,'80008a');
/*!40000 ALTER TABLE `t_mood` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `user`
--

DROP TABLE IF EXISTS `user`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `user` (
  `u` varchar(50) DEFAULT NULL,
  `p` varchar(50) DEFAULT NULL,
  `sq` varchar(50) DEFAULT NULL,
  `sa` varchar(50) DEFAULT NULL,
  `uID` int(11) DEFAULT NULL,
  `connectionID` varchar(50) DEFAULT NULL,
  `isOnline` int(11) DEFAULT NULL,
  `buddyList` varchar(100) DEFAULT NULL,
  `phoneStatus` int(11) DEFAULT NULL,
  `chatStatus` int(11) DEFAULT NULL,
  `transactionCount` int(11) DEFAULT NULL,
  `transactionHistory` mediumtext DEFAULT NULL,
  `jammersTotal` int(11) DEFAULT NULL,
  `jammersUsed` int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `user`
--

LOCK TABLES `user` WRITE;
/*!40000 ALTER TABLE `user` DISABLE KEYS */;
INSERT INTO `user` VALUES ('GUESTUSER',NULL,NULL,NULL,0,NULL,0,NULL,NULL,NULL,NULL,NULL,NULL,NULL);
/*!40000 ALTER TABLE `user` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2023-02-14 21:47:52
