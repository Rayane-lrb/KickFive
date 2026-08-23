using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KickFive.Models;

public class FieldsController : Controller
{
    private readonly KickFiveContext _context;

    public FieldsController(KickFiveContext context)
    {
        _context = context;
    }

    // GET: FIELDS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Field.ToListAsync());
    }

    // GET: FIELDS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var field = await _context.Field
            .FirstOrDefaultAsync(m => m.Id == id);
        if (field == null)
        {
            return NotFound();
        }

        return View(field);
    }

    // GET: FIELDS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: FIELDS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Name")] Field field)
    {
        if (ModelState.IsValid)
        {
            _context.Add(field);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(field);
    }

    // GET: FIELDS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var field = await _context.Field.FindAsync(id);
        if (field == null)
        {
            return NotFound();
        }
        return View(field);
    }

    // POST: FIELDS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,Name")] Field field)
    {
        if (id != field.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(field);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FieldExists(field.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(field);
    }

    // GET: FIELDS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var field = await _context.Field
            .FirstOrDefaultAsync(m => m.Id == id);
        if (field == null)
        {
            return NotFound();
        }

        return View(field);
    }

    // POST: FIELDS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var field = await _context.Field.FindAsync(id);
        if (field != null)
        {
            _context.Field.Remove(field);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool FieldExists(int? id)
    {
        return _context.Field.Any(e => e.Id == id);
    }
}
