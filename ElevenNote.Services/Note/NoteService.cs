using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using ElevenNote.Models.Note;
using Microsoft.EntityFrameworkCore;
using ElevenNote.Data;
using ElevenNote.Data.Entities;
using AutoMapper;

namespace ElevenNote.Services.Note
{
    public class NoteService : INoteService
    {
        private readonly int _userId;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _dbContext;

        public NoteService(IHttpContextAccessor httpContextAccessor, IMapper mapper, ApplicationDbContext dbContext)
        {
            var userClaims = httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
            var value = userClaims.FindFirst("Id")?.Value;
            var validId = int.TryParse(value, out _userId);

            if(!validId)
            {
                throw new Exception("Attempted to build NoteService without User Id claim");
            }

            _mapper = mapper;
            _dbContext = dbContext;
        }

        public async Task<bool> CreateNoteAsync(NoteCreate request)
        {
            var noteEntity = _mapper.Map<NoteCreate, NoteEntity>(request, opt =>
                opt.AfterMap((src, dest) => dest.OwnerId = _userId)
            );
            // var noteEntity = new NoteEntity
            // {
            //     Title = request.Title,
            //     Content = request.Content,
            //     CreatedUtc = DateTimeOffset.Now,
            //     OwnerId = _userId
            // };

            _dbContext.Notes.Add(noteEntity);

            var numberOfChanges = await _dbContext.SaveChangesAsync();
            return numberOfChanges == 1;
        }

        public async Task<IEnumerable<NoteListItem>> GetAllNotesAsync()
        {
            var notes = await _dbContext.Notes
            .Where(entity => entity.OwnerId == _userId)
            .Select(entity => _mapper.Map<NoteListItem>(entity))
            .ToListAsync();
            // var notes = await _dbContext.Notes.Where(entity => entity.OwnerId == _userId)
            // .Select(entity => new NoteListItem 
            // {
            //     Id = entity.Id,
            //     Title = entity.Title,
            //     CreatedUtc = entity.CreatedUtc
            // }).ToListAsync();

            return notes;
        }

        public async Task<NoteDetail> GetNoteByIdAsync(int noteId)
        {
            //find the first note that has the given Id and an OwnerId that matches the requesting userId
            var noteEntity = await _dbContext.Notes.FirstOrDefaultAsync(e => e.Id == noteId && e.OwnerId == _userId);

            //If noteEntity is null then return null, otherwise initialize and return a new NoteDetail
            return noteEntity is null ? null : _mapper.Map<NoteDetail>(noteEntity);
            // return noteEntity is null ? null : new NoteDetail
            // {
            //     Id = noteEntity.Id,
            //     Title = noteEntity.Title,
            //     Content = noteEntity.Content,
            //     CreatedUtc = noteEntity.CreatedUtc,
            //     ModifiedUtc = noteEntity.ModifiedUtc
            // };
        }

        public async Task<bool> UpdateNoteAsync(NoteUpdate request)
        {
            //Check the database to see if there's a note entity that matches the request informaiton
            //Any returns true if any entity exists
            var noteIsUserOwned = await _dbContext.Notes.AnyAsync(note =>
            note.Id == request.Id && note.OwnerId == _userId
            );

            //If the Any check returns false then we know the Note either does not exist
            //Or the note is not owned by the user
            if (!noteIsUserOwned)
            {
                return false;
            }

            //Map from Update to Entity and set OwnerId again
            var newEntity = _mapper.Map<NoteUpdate, NoteEntity>(request, opt => 
                opt.AfterMap((src, dest) => dest.OwnerId = _userId)
                );

                //Update the Entry state, which is another way to tell the db context something changed
                _dbContext.Entry(newEntity).State = EntityState.Modified;

                //Bc don't currently have access to createdUtc value, just mark it as not modified
                _dbContext.Entry(newEntity).Property(e => e.CreatedUtc).IsModified = false;

                //Save the changes to the database and capture how many rows were updated
                var numberOfChanges = await _dbContext.SaveChangesAsync();
                
                //numberofChanges is state to be equal to 1 because only one row is updated
                return numberOfChanges == 1;
        }

        public async Task<bool> DeleteNoteAsync(int noteId)
        {
            //Find the note by the given Id
            var noteEntity = await _dbContext.Notes.FindAsync(noteId);

            //Validate the note exists and is owned by the user
            if(noteEntity?.OwnerId != _userId)
            {
                return false;
            }

            //Remove the note from the DbContext and assert that the one change was saved
            _dbContext.Notes.Remove(noteEntity);
            return await _dbContext.SaveChangesAsync() == 1;
        }
    }
}