using APIMinima;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList")); 
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();

RouteGroupBuilder todoItems = app.MapGroup("/todoitems");

todoItems.MapGet("/", GetAllTodos);
todoItems.MapGet("/complete", GetCompleteTodos);
todoItems.MapGet("/{id}", GetTodo);
todoItems.MapPost("/", CreateTodo);
todoItems.MapPut("/{id}", UpdateTodo);
todoItems.MapDelete("/{id}", DeleteTodo);

app.Run(); 

static async Task<IResult> GetAllTodos(TodoDb db)
{
    return TypedResults.Ok(await db.Todos.Select(x => new TodoItemDTO(x)).ToArrayAsync());
} 

static async Task<IResult> GetCompleteTodos(TodoDb db)
{
    return TypedResults.Ok(await db.Todos.Where(t => t.IsComplete).Select(x => new TodoItemDTO(x)).ToListAsync());
} 

static async Task<IResult> GetTodo (int id, TodoDb db)
{
    return await db.Todos.FindAsync(id)
        is Todo todo
            ? TypedResults.Ok(new TodoItemDTO(todo))
            : TypedResults.NotFound();
} 

static async Task<IResult> CreateTodo(TodoItemDTO todoItemDTO, TodoDb db)
{
    //Console.WriteLine(todoItemDTO);

    var todoItem = new Todo()
    {
        IsComplete = todoItemDTO.IsComplete,
        Name = todoItemDTO.Name
    };

    //Console.WriteLine(todoItem);

    db.Todos.Add(todoItem); 
    await db.SaveChangesAsync();  

    todoItemDTO = new TodoItemDTO(todoItem);

    return TypedResults.Created($"/todoItems/{todoItem.Id}", todoItemDTO);
} 

static async Task<IResult> UpdateTodo(int id, TodoItemDTO TodoItemDTO, TodoDb db)
{
    var todo = await db.Todos.FindAsync(id); 

    if(todo is null) return TypedResults.NotFound(); 

    todo.Name = TodoItemDTO.Name;
    todo.IsComplete = TodoItemDTO.IsComplete;

    await db.SaveChangesAsync();

    return TypedResults.NoContent();

} 

static async Task<IResult> DeleteTodo(int id, TodoDb db)
{
    if(await db.Todos.FindAsync(id) is Todo todo)
    {
        db.Todos.Remove(todo); 
        await db.SaveChangesAsync(); 

        TodoItemDTO todoItemDTO = new TodoItemDTO(todo);

        return TypedResults.Ok(todoItemDTO);
    } 

    return TypedResults.NotFound();
}

////-----------------------------------------------------------GET----------------------------------------------------------------
//todoItems.MapGet("/", async(TodoDb db) => 
//    await db.Todos.ToListAsync());


//todoItems.MapGet("/complete", async (TodoDb db) =>
//    await db.Todos.Where(t => t.IsComplete).ToListAsync());


//todoItems.MapGet("/{id}", async(int id, TodoDb db) =>  
//    await db.Todos.FindAsync(id) 
//    is Todo todo ? Results.Ok(todo) : Results.NotFound());

////-----------------------------------------------------------POST----------------------------------------------------------------
//todoItems.MapPost("/", async (Todo todo, TodoDb db) =>
//{
//    db.Todos.Add(todo);
//    await db.SaveChangesAsync();

//    return Results.Created($"/todoitems/{todo.Id}", todo);
//});
////-----------------------------------------------------------PUT---------------------------------------------------------------- 
//todoItems.MapPut("/{id}", async (int id, Todo inputTodo, TodoDb db) =>
//{  
//    var todo = await db.Todos.FindAsync(id); 

//    if (todo is null) return Results.NotFound(); 

//    todo.Name = inputTodo.Name; 
//    todo.IsComplete = inputTodo.IsComplete; 

//    await db.SaveChangesAsync(); 

//    return Results.NoContent();
//});
////-----------------------------------------------------------DELETE----------------------------------------------------------------
//todoItems.MapDelete("/{id}", async (int id, TodoDb db) =>
//{ 
//    if(await db.Todos.FindAsync(id) is Todo todo)
//    {
//        db.Todos.Remove(todo); 
//        await db.SaveChangesAsync();

//        return Results.Ok(todo);
//    } 

//    return Results.NotFound();
//});