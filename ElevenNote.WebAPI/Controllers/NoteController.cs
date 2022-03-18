using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ElevenNote.Services.Note;
using Microsoft.AspNetCore.Authorization;
using ElevenNote.Models.Note;

namespace ElevenNote.WebAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class NoteController : ControllerBase
    {
        private readonly INoteService _noteService;

        public NoteController(INoteService noteService)
        {
            _noteService = noteService;
        }

        //Post api/Note
        [HttpPost]
        public async Task<IActionResult> CreateNote([FromBody] NoteCreate request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if(await _noteService.CreateNoteAsync(request))
            {
                return Ok("Note created successfully.");
            }

            return BadRequest("Note could not be created");
        }

        //Get api/Note
        [HttpGet]
        public async Task<IActionResult> GetAllNotes()
        {
            var notes = await _noteService.GetAllNotesAsync();
            return Ok(notes);
        }

        //Get api/Note/5
        [HttpGet("{noteId:int}")]
        public async Task<IActionResult> GetNoteById([FromRoute] int noteId)
        {
            var detail = await _noteService.GetNoteByIdAsync(noteId);
            
            //Similar to our service method, we're using a ternary to determine our return type
            //if the returned value (detail) is not null, return it with a 200 OK
            //otherwise return a notfound() 404 response
            return detail is not null ? Ok(detail) : NotFound();
        }


    }
}