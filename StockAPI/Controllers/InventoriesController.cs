using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockAPI.Data;
using StockAPI.Model;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Data;
using System.ComponentModel.Design;
using Microsoft.AspNetCore.Authorization;

namespace StockAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class InventoriesController : ControllerBase
    {
        private readonly DataContext _context;

        public InventoriesController(DataContext context)
        {
            _context = context;
        }

        // GET: api/Inventories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CompanyInventory>>> GetInventories()
        {
            return await _context.Inventories.ToListAsync();
        }

        // GET: api/Inventories
        [HttpGet]
        [Route("ByCompany/{companyId:int}")]
        public async Task<ActionResult<IEnumerable<CompanyInventory>>> GetCompanyInventories(int companyId)
        {
            return await _context.Inventories.Where( x => x.CompanyNit == companyId).ToListAsync();
        }

        public readonly record struct MailData (string receptor, int nitCompany);

        [HttpPost]
        [Route("sendmail")]
        public async Task<IActionResult> SendMail(MailData mailData)
        {
            var inventories = _context.Inventories.Where(x => x.CompanyNit == mailData.nitCompany).ToList();

            if (inventories.Any())
            {
                string encodedPDF = "";
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    Document document = new Document(PageSize.A4);
                    PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
                    document.Open();
                    Font font5 = FontFactory.GetFont(FontFactory.HELVETICA, 5);

                    PdfPTable table = new PdfPTable(2);
                    float[] widths = new float[2];

                    for (int i = 0; i < 2; i++) { widths[i] = 4f; }
                    table.SetWidths(widths);
                    table.WidthPercentage = 100;

                    PdfPCell cell = new PdfPCell(new Phrase("InventoryReport"));

                    cell.Colspan = 2;

                    table.AddCell(new Phrase("Name", font5));
                    table.AddCell(new Phrase("Quantity", font5));

                    foreach (var inventory in inventories)
                    {
                        table.AddCell(new Phrase(inventory.Name, font5));
                        table.AddCell(new Phrase(inventory.Quantity.ToString(), font5));
                    }
                    document.Add(table);

                    document.Close();

                    var bytes = memoryStream.ToArray();
                    encodedPDF = Convert.ToBase64String(bytes);
                }


                //var apiKey = Environment.GetEnvironmentVariable("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");
                var client = new SendGridClient("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");
                var from = new EmailAddress("leon.aleck@fallinhay.com", "Example User");
                var subject = "Test mail: Developer C#";
                var to = new EmailAddress(mailData.receptor, "Test user");
                var plainTextContent = $"and easy to do anywhere, even with C# {mailData.nitCompany}";
                var htmlContent = "<strong>This is a email send, to a developer C# test</strong>";
                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                msg.AddAttachment("InventoryReport.pdf", encodedPDF, type: "application/pdf");
                var response = await client.SendEmailAsync(msg);
                return Ok(new { message = "Mail sent." });
            }
            return Ok(new { message = "No inventaries to send." });
        }

        // GET: api/Inventories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CompanyInventory>> GetCompanyInventory(int id)
        {
            var companyInventory = await _context.Inventories.FindAsync(id);

            if (companyInventory == null)
            {
                return NotFound();
            }

            return companyInventory;
        }

        // PUT: api/Inventories/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCompanyInventory(int id, CompanyInventory companyInventory)
        {
            if (id != companyInventory.Id)
            {
                return BadRequest();
            }

            _context.Entry(companyInventory).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CompanyInventoryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Inventories
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<CompanyInventory>> PostCompanyInventory(CompanyInventory companyInventory)
        {
            _context.Inventories.Add(companyInventory);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCompanyInventory", new { id = companyInventory.Id }, companyInventory);
        }

        // DELETE: api/Inventories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCompanyInventory(int id)
        {
            var companyInventory = await _context.Inventories.FindAsync(id);
            if (companyInventory == null)
            {
                return NotFound();
            }

            _context.Inventories.Remove(companyInventory);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CompanyInventoryExists(int id)
        {
            return _context.Inventories.Any(e => e.Id == id);
        }
    }
}
