﻿using MauiAppMinhasCompras.Models;
using SQLite;

namespace MauiAppMinhasCompras.Helpers
{
    public class SQLiteDatabaseHelper
    {
        readonly SQLiteAsyncConnection _conn;

        public SQLiteDatabaseHelper(string path)
        {
            _conn = new SQLiteAsyncConnection(path);
            _conn.CreateTableAsync<Produto>().Wait();
        }

        public Task<int> Insert(Produto p)
        {
            return _conn.InsertAsync(p);
        }

        public Task<List<Produto>> Update(Produto p)
        {
            string sql = "UPDATE Produto SET Descricao=?, Quantidade=?, Preco=?, Categoria=? WHERE Id=?";

            return _conn.QueryAsync<Produto>(
                sql, p.Descricao, p.Quantidade, p.Preco, p.Categoria, p.Id
            );
        }

        public Task<int> Delete(int id)
        {
            return _conn.Table<Produto>().DeleteAsync(i => i.Id == id);
        }

        public Task<List<Produto>> GetAll()
        {
            return _conn.Table<Produto>().ToListAsync();
        }

        public Task<List<Produto>> Search(string q)
        {
            string sql = "SELECT * FROM Produto WHERE descricao LIKE '%" + q + "%'";

            return _conn.QueryAsync<Produto>(sql);
        }

        public Task<List<string>> GetCategorias()
        {
            string sql = "SELECT DISTINCT Categoria FROM Produto";
            return _conn.QueryScalarsAsync<string>(sql);
        }

        public Task<List<Produto>> GetByCategoria(string categoria)
        {
            string sql = "SELECT * FROM Produto WHERE Categoria = ?";
            return _conn.QueryAsync<Produto>(sql, categoria);
        }

        public Task<List<Produto>> SearchByDescricaoECategoria(string descricao, string categoria)
        {
            string sql = "SELECT * FROM Produto WHERE Descricao LIKE ? AND Categoria = ?";
            return _conn.QueryAsync<Produto>(sql, "%" + descricao + "%", categoria);
        }

        public async Task<Dictionary<string, double>> GetTotalPorCategoria()
        {
            var produtos = await _conn.Table<Produto>().ToListAsync();

            return produtos
                .GroupBy(p => p.Categoria)
                .ToDictionary(g => g.Key, g => g.Sum(p => p.Total));
        }
    }
}