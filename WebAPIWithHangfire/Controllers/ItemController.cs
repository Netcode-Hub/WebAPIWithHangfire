using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace WebAPIWithHangfire.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ItemController : ControllerBase
    {
        private readonly AppDbContext conn;
        public ItemController(AppDbContext conn)
        {
            this.conn = conn;
        }

        // Background Jobs
        //Display message(object) As soon as  post is made.

        [HttpPost("BackgroundJob-Enqueue")]
        public async Task<IActionResult> Post([FromBody] Item item)
        {
            conn.Items.Add(item);
            await conn.SaveChangesAsync();
            BackgroundJob.Enqueue(() => DisplayBackground(item));
            return Ok(true);
        }
        public static void DisplayBackground(Item item)
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("This is the Item you just added");
            Console.WriteLine("Name: " + item.Name);
            Console.WriteLine("Quantity: " + item.Quantity + Environment.NewLine);
        }


        //Schedule Jobs
        // Display list of items 8 seconds past after calling this method. 

        [HttpGet("BackgroundJob-Schedule")]
        public async Task<IActionResult> Get()
        {
            var items = await conn.Items.ToListAsync();
            BackgroundJob.Schedule(() => DoConsoleStuff(items), new DateTimeOffset(DateTime.UtcNow.AddSeconds(8)));
            return Ok(items);
        }
        public static void DoConsoleStuff(List<Item> items)
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine($"[Name  Quantity]");
            foreach (var item in items)
            {
                Console.WriteLine($"[{item.Name} ----- {item.Quantity}]");
            }
            Console.WriteLine(Environment.NewLine);
        }



        //Continuation Job
        // Display items total quantity 10 times, it first needs started job's Id to check if it has finished then it continues.

        [HttpGet("BackgroundJob-ContinueJobWith")]
        public async Task<IActionResult> TotalQuantity()
        {
            var items = await conn.Items.ToListAsync();
            var DefaultJobId = BackgroundJob.Schedule(() => DoConsoleStuff(items), new DateTimeOffset(DateTime.UtcNow.AddSeconds(5)));
            await Task.Delay(5000);
            var newjob1 = BackgroundJob.ContinueJobWith(DefaultJobId, () => GetItemQuantity(items));
            await Task.Delay(7000);
            var newjob2 = BackgroundJob.ContinueJobWith(newjob1, () => DoConsoleStuff(items));
            await Task.Delay(8000);
            var newjob3 = BackgroundJob.ContinueJobWith(newjob2, () => GetItemQuantity(items));
            await Task.Delay(9000);
            var newjob4 = BackgroundJob.ContinueJobWith(newjob3, () => DoConsoleStuff(items));
            await Task.Delay(1000);
            var newjob5 = BackgroundJob.ContinueJobWith(newjob4, () => GetItemQuantity(items));
            return Ok(items.Sum(_=>_.Quantity));
        }
        public static void GetItemQuantity(List<Item> items)
        {
            Console.WriteLine(Environment.NewLine + "Total Quantities: " + items.Sum(_ => _.Quantity) + Environment.NewLine);
        }





        //Recurring Job
        // Display the names in List and quantities

        [HttpGet("Recurring-Jobs")]
        public async Task<IActionResult> GetItems()
        {
            var items = await conn.Items.ToListAsync();
            RecurringJob.AddOrUpdate("RecurrigJobId", () => DoConsoleStuff(items), "* * * * * *");
            return Ok(items);
        }
    }
}
