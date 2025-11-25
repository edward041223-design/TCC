CREATE DATABASE  IF NOT EXISTS `cardapio_db` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;
USE `cardapio_db`;
-- MySQL dump 10.13  Distrib 8.0.43, for Win64 (x86_64)
--
-- Host: localhost    Database: cardapio_db
-- ------------------------------------------------------
-- Server version	9.1.0

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
-- Dumping data for table `__efmigrationshistory`
--

LOCK TABLES `__efmigrationshistory` WRITE;
/*!40000 ALTER TABLE `__efmigrationshistory` DISABLE KEYS */;
INSERT INTO `__efmigrationshistory` VALUES ('20251008032331_MigracaoTokenFinal','8.0.0');
/*!40000 ALTER TABLE `__efmigrationshistory` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping data for table `pratos`
--

LOCK TABLES `pratos` WRITE;
/*!40000 ALTER TABLE `pratos` DISABLE KEYS */;
INSERT INTO `pratos` VALUES (1,'Bebidas','Refrigerante','[\'confidencial\']',2.55,'Desconhecido'),(2,'Entradas','Dip de Espinafre e Alcachofra','[\'Tomates\', \'Manjericão\', \'Alho\', \'Azeite de Oliva\']',11.12,'Não'),(3,'Sobremesas','Cheesecake de NÃ£ova York','[\'Chocolate\', \'Manteiga\', \'Açúcar\', \'Ovos\']',18.66,'Sim'),(4,'Prato Principal','Frango Alfredo','[\'Frango\', \'Fettuccine\', \'Molho Alfredo\', \'ParmesÃ£o\']',29.55,'Sim'),(5,'Prato Principal','Bife Grelhado','[\'Frango\', \'Fettuccine\', \'Molho Alfredo\', \'ParmesÃ£o\']',17.73,'Sim'),(6,'Entradas','Cogumelos Recheados','[\'Tomates\', \'Manjericão\', \'Alho\', \'Azeite de Oliva\']',12.28,'Não'),(7,'Sobremesas','Tiramisu','[\'Chocolate\', \'Manteiga\', \'Açúcar\', \'Ovos\']',10.47,'Desconhecido'),(8,'Bebidas','Limonada','[\'confidencial\']',4.95,'Sim'),(9,'Sobremesas','Bolo VulcÃ£o de Chocolate','[\'Chocolate\', \'Manteiga\', \'Açúcar\', \'Ovos\']',16.08,'Sim'),(10,'Bebidas','Iced Tea','[\'confidencial\']',4.43,'Desconhecido'),(11,'Bebidas','Coffee','[\'confidencial\']',2.94,'Sim'),(12,'Entradas','Bruschetta','[\'Tomates\', \'Manjericão\', \'Alho\', \'Azeite de Oliva\']',13.62,'Não'),(13,'Prato Principal','Refogado de Vegetais','[\'Frango\', \'Fettuccine\', \'Molho Alfredo\', \'ParmesÃ£o\']',27.06,'Desconhecido'),(14,'Prato Principal','Shrimp Scampi','[\'Frango\', \'Fettuccine\', \'Molho Alfredo\', \'ParmesÃ£o\']',19.87,'Sim'),(15,'Sobremesas','Fruit Tart','[\'Chocolate\', \'Manteiga\', \'Açúcar\', \'Ovos\']',12.38,'Desconhecido'),(16,'Entradas','Caprese Salad','[\'Tomates\', \'Manjericão\', \'Alho\', \'Azeite de Oliva\']',14.47,'Sim');
/*!40000 ALTER TABLE `pratos` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping data for table `usuarios`
--

LOCK TABLES `usuarios` WRITE;
/*!40000 ALTER TABLE `usuarios` DISABLE KEYS */;
INSERT INTO `usuarios` VALUES (1,'Pedro Testador','pedro.teste@gmail.com','Senha12345','N/A','Padrão','Nenhuma','2025-10-11 16:18:19.009752'),(2,'Mary','Mary@gmail.com','Senha123456','N/A','Padrão','Nenhuma','2025-10-11 17:36:40.605097'),(3,'Edu Teste','edu@gmail.com','654321','N/A','Padrão','Nenhuma','2025-10-11 20:05:57.578474'),(4,'Novo Usuário','novo@gmail.com','senha456','11999999999','Vegetariano','Lactose','2025-10-12 11:45:02.381686'),(5,'Maria','maria.teste@hotmaill.com','7654321','999999999','Nao gosto de milho','Lactose','2025-10-12 12:24:26.589936'),(6,'edu Teste','edu.teste@gmail.com','Senha98765','11111111','nao gosto de milho','lactose','2025-10-12 12:41:53.351777'),(7,'kaka','kaka@gmail.com','senha123','11999999999','Vegetariano','Lactose','2025-10-20 11:05:52.307853'),(8,'murilo','murilo@gmail.com','54321','44444','gosto de carne','nada','2025-10-31 22:18:07.788360'),(9,'elano','elano07@gmail.com','12345','99999999','tem que ter milho','lactose','2025-10-31 23:03:42.186249'),(10,'Pedro Sanchez','pedro@gmail.com','senha543321','8888888','nao gosto de milho','lactose','2025-11-02 15:15:57.376761'),(11,'ana maria','ana@hotmail.com','Senha555','9999999','nao gosto de milho','lactose','2025-11-02 18:09:58.880051'),(12,'mary','mary06','111111','777777','nao gosto de ervilha','lactose','2025-11-02 18:49:49.872555'),(13,'João Silva','teste@gmail.com','123456','11999999999','Comidas leves','Nenhuma','2025-11-03 15:34:14.918581'),(14,'Maria Souza','maria@teste.com','12345','11988887777','vegetariano','amendoim','2025-11-07 18:29:55.340138'),(15,'Auu','du@gmail.com','777777','208088','carne','nenhuma','2025-11-09 17:45:16.111517'),(17,'Dudu','dudu@gmail.com','123456','11999999999','Tomates, Alho','lactose','2025-11-11 20:39:55.192916'),(18,'Eduardo Teste','eduardo.teste@email.com','123','11987654321','','Lactose','2025-11-12 19:51:04.753176'),(19,'Rose','rose@gmail.com','55555','123456789','Frango, Manjericão','Lactose','2025-11-21 15:46:30.553952');
/*!40000 ALTER TABLE `usuarios` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-11-24 11:30:11
