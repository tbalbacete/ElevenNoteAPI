using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ElevenNote.Data;
using ElevenNote.Data.Entities;
using ElevenNote.Models.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ElevenNote.Services.User
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        //Register user
        public async Task<bool> RegisterUserAsync(UserRegister model)
        {
            //check to make sure email and username are valid prior to creating entity
            if (await GetUserByEmailAsync(model.Email) !=null || await GetUserByUsernameAsync(model.Username) != null)
            {
                return false;
            }
            
            //create entity of user
            var entity = new UserEntity
            {
                Email = model.Email,
                Username = model.Username,
                DateCreated = DateTime.Now
            };

            var passwordHasher = new PasswordHasher<UserEntity>();

            entity.Password = passwordHasher.HashPassword(entity, model.Password);

            //Add user to ApplicationDbContext
            _context.Users.Add(entity);

            //return bool for the change completed
            var numberOfChanges = await _context.SaveChangesAsync();
            return numberOfChanges == 1;
        }

        //check that email is unique
        private async Task<UserEntity> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(user => user.Email.ToLower() == email.ToLower());
        }

        //check that username is unique
        private async Task<UserEntity> GetUserByUsernameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(user => user.Username.ToLower() == username.ToLower());
        }
    }
}