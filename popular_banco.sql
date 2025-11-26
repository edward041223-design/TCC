-- ============================================================
-- Script para popular banco de dados do Cardápio Inteligente
-- Database: cardapio_db
-- ============================================================

-- Criar banco se não existir
CREATE DATABASE IF NOT EXISTS cardapio_db;
USE cardapio_db;

-- ============================================================
-- PRATOS SEM LACTOSE (Para recomendar)
-- ============================================================

INSERT INTO Pratos (Nome, Descricao, TemLactose, Preco, Categoria) VALUES
-- Saladas
('Salada Caesar Vegana', 'Alface romana, croutons, molho caesar sem lactose à base de castanhas', 0, 18.50, 'Saladas'),
('Salada Tropical', 'Mix de folhas, manga, abacaxi, castanhas e molho de maracujá', 0, 16.00, 'Saladas'),
('Salada Quinoa', 'Quinoa, tomate cereja, pepino, azeitonas e azeite de oliva', 0, 19.00, 'Saladas'),

-- Carnes e Peixes
('Frango Grelhado', 'Peito de frango temperado com ervas finas, sem manteiga', 0, 22.00, 'Pratos Principais'),
('Salmão ao Molho de Alcaparras', 'Salmão grelhado com molho sem creme de leite', 0, 35.00, 'Pratos Principais'),
('Picanha Nobre', 'Picanha grelhada mal passada, acompanha arroz e farofa', 0, 42.00, 'Pratos Principais'),
('Tilápia Assada', 'Tilápia com temperos naturais e legumes', 0, 28.00, 'Pratos Principais'),

-- Massas
('Espaguete ao Alho e Óleo', 'Massa italiana, alho dourado, azeite extra virgem e pimenta', 0, 24.00, 'Massas'),
('Penne ao Pesto de Manjericão', 'Penne integral, pesto sem queijo, tomate seco', 0, 26.00, 'Massas'),
('Nhoque de Batata com Molho de Tomate', 'Nhoque artesanal (sem leite) e molho pomodoro', 0, 23.00, 'Massas'),

-- Pratos Veganos
('Bowl Buddha', 'Grão-de-bico, quinoa, abóbora assada, espinafre e tahine', 0, 25.00, 'Veganos'),
('Hambúrguer de Lentilha', 'Hambúrguer vegetal, pão sem lactose, salada', 0, 21.00, 'Veganos'),
('Curry de Legumes', 'Legumes variados ao curry com leite de coco', 0, 23.00, 'Veganos'),

-- Sobremesas sem Lactose
('Brownie Vegano', 'Brownie de chocolate amargo sem leite nem ovos', 0, 12.00, 'Sobremesas'),
('Sorvete de Coco', 'Sorvete artesanal à base de leite de coco', 0, 14.00, 'Sobremesas'),
('Mousse de Chocolate Vegano', 'Mousse cremoso com chocolate 70% cacau e leite de amêndoas', 0, 13.50, 'Sobremesas'),
('Salada de Frutas', 'Mix de frutas da estação com calda de maracujá', 0, 11.00, 'Sobremesas');

-- ============================================================
-- PRATOS COM LACTOSE (Para comparação/alerta)
-- ============================================================

INSERT INTO Pratos (Nome, Descricao, TemLactose, Preco, Categoria) VALUES
-- Pratos com Queijo
('Lasanha à Bolonhesa', 'Massa intercalada com molho bolonhesa e MUITO QUEIJO', 1, 28.00, 'Massas'),
('Pizza Quatro Queijos', 'Mussarela, parmesão, gorgonzola e provolone', 1, 32.00, 'Pizzas'),
('Risoto de Queijo Brie', 'Arroz arbóreo com QUEIJO BRIE cremoso', 1, 34.00, 'Pratos Principais'),

-- Pratos com Creme de Leite
('Strogonoff de Carne', 'Filé mignon ao molho com CREME DE LEITE', 1, 36.00, 'Pratos Principais'),
('Fettuccine Alfredo', 'Massa ao molho BRANCO com CREME DE LEITE e parmesão', 1, 29.00, 'Massas'),

-- Sobremesas com Lactose
('Pudim de Leite', 'Pudim tradicional com LEITE CONDENSADO', 1, 10.00, 'Sobremesas'),
('Cheesecake', 'Torta com base de biscoito e CREAM CHEESE', 1, 15.00, 'Sobremesas'),
('Petit Gateau', 'Bolo de chocolate com sorvete de CREME', 1, 18.00, 'Sobremesas'),
('Torta de Limão', 'Torta com creme de LEITE CONDENSADO', 1, 14.00, 'Sobremesas');

-- ============================================================
-- USUÁRIO DE TESTE (senha: teste123)
-- Hash BCrypt de "teste123": $2a$11$exemplo...
-- ============================================================

-- Nota: O hash abaixo é um exemplo. Na prática, será gerado pelo app
INSERT IGNORE INTO Usuarios (Nome, Email, SenhaHash, DataCriacao) VALUES
('Usuário Teste', 'teste@cardapio.com', '$2a$11$N2zqVhzqzVhzqzVhzqzVhO', NOW()),
('Maria Silva', 'maria@example.com', '$2a$11$N2zqVhzqzVhzqzVhzqzVhO', NOW());

-- ============================================================
-- VERIFICAÇÃO
-- ============================================================

SELECT 'Pratos SEM lactose cadastrados:' as Info, COUNT(*) as Total FROM Pratos WHERE TemLactose = 0;
SELECT 'Pratos COM lactose cadastrados:' as Info, COUNT(*) as Total FROM Pratos WHERE TemLactose = 1;
SELECT 'Total de pratos:' as Info, COUNT(*) as Total FROM Pratos;
SELECT 'Usuários cadastrados:' as Info, COUNT(*) as Total FROM Usuarios;

-- Exibir alguns pratos sem lactose
SELECT Nome, Preco, Categoria FROM Pratos WHERE TemLactose = 0 LIMIT 5;

-- ============================================================
-- CONSULTAS ÚTEIS PARA TESTES
-- ============================================================

-- Ver todos os pratos sem lactose
-- SELECT * FROM Pratos WHERE TemLactose = 0;

-- Ver pratos por categoria
-- SELECT Categoria, COUNT(*) as Quantidade FROM Pratos GROUP BY Categoria;

-- Ver pratos mais caros
-- SELECT Nome, Preco FROM Pratos ORDER BY Preco DESC LIMIT 5;

-- Buscar pratos por nome
-- SELECT * FROM Pratos WHERE Nome LIKE '%vegano%';
