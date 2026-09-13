using System.Diagnostics;
using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Mvc;
using Todo.Models;
using Todo.Models.ViewModels;

namespace Todo.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        var todoListViewModel = GetAllTodos();
        return View(todoListViewModel);
    }

    internal TodoViewModel GetAllTodos()
    {
        List<TodoItem> todoList = new();

        using var con = new SqliteConnection("Data Source=db.sqlite");
        using var tableCmd = con.CreateCommand();

        tableCmd.CommandText = "SELECT * FROM todo";

        con.Open();
        using var reader = tableCmd.ExecuteReader();

        while (reader.Read())
        {
            todoList.Add(new TodoItem
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1)
            });
        }

        return new TodoViewModel
        {
            TodoList = todoList
        };
    }

    [HttpPost]
    public IActionResult Insert(TodoItem todo)
    {
        using var con = new SqliteConnection("Data Source=db.sqlite");
        using var tableCmd = con.CreateCommand();

        tableCmd.CommandText = "INSERT INTO todo (name) VALUES (@Name)";
        tableCmd.Parameters.AddWithValue("@Name", todo.Name);

        try
        {
            con.Open();
            tableCmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        // Riktig måte å sende brukeren tilbake til Index-metoden
        return RedirectToAction("Index");
    }
    [HttpPost]
    public IActionResult Update(TodoItem todo)
    {
        using var con = new SqliteConnection("Data Source=db.sqlite");
        using var tableCmd = con.CreateCommand();

        tableCmd.CommandText = "UPDATE todo SET name = @Name WHERE id = @Id";
        tableCmd.Parameters.AddWithValue("@Name", todo.Name);
        tableCmd.Parameters.AddWithValue("@Id", todo.Id);

        try
        {
            con.Open();
            tableCmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        return RedirectToAction("Index");
    }

    [HttpGet]
    public JsonResult PopulateForm(int id)
    {
        var todo = GetById(id);
        return Json(todo);
    }

    internal TodoItem? GetById(int id)
    {
        using var connection = new SqliteConnection("Data Source=db.sqlite");
        using var tableCmd = connection.CreateCommand();

        tableCmd.CommandText = "SELECT * FROM todo WHERE Id = @Id";
        tableCmd.Parameters.AddWithValue("@Id", id);

        connection.Open();
        using var reader = tableCmd.ExecuteReader();

        if (reader.Read())
        {
            return new TodoItem
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1)
            };
        }
        return null;
    }

    [HttpPost]
    public JsonResult Delete(int id)
    {
        using var con = new SqliteConnection("Data Source=db.sqlite");
        using var tableCmd = con.CreateCommand();

        // Oppdatert med parametere og 'using var'
        tableCmd.CommandText = "DELETE FROM todo WHERE Id = @Id";
        tableCmd.Parameters.AddWithValue("@Id", id);

        con.Open();
        tableCmd.ExecuteNonQuery();

        return Json(new { success = true });
    }
}