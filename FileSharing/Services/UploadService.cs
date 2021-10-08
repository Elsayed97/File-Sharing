using AutoMapper;
using AutoMapper.QueryableExtensions;
using FileSharing.Data;
using FileSharing.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileSharing.Services
{

        public class UploadService : IUploadService
        {
            private readonly ApplicationDbContext _db;
            private readonly IMapper _mapper;

            public UploadService(ApplicationDbContext context, IMapper mapper)
            {
                _db = context;
                _mapper = mapper;
            }
            public async Task CreateAsync(InputUpload model)
            {
                var mappedObj = _mapper.Map<Uploads>(model);
                await _db.Uploads.AddAsync(mappedObj);
                await _db.SaveChangesAsync();
            }

            public async Task DeleteAsync(string id, string userId)
            {
                var selectedUpload = await _db.Uploads.FirstOrDefaultAsync(u => u.Id == id && u.UserId == userId);
                if (selectedUpload != null)
                {
                    _db.Uploads.Remove(selectedUpload);
                    await _db.SaveChangesAsync();
                }
            }

            public async Task<UploadViewModel> FindAsync(string id, string userId)
            {
                var selectedUpload = await _db.Uploads.FirstOrDefaultAsync(u => u.Id == id && u.UserId == userId);
                if (selectedUpload != null)
                {
                    //AutoMapper 
                    return _mapper.Map<UploadViewModel>(selectedUpload);
                }
                return null;
            }

            public async Task<UploadViewModel> FindAsync(string id)
            {
                var selectedUpload = await _db.Uploads.FindAsync(id);
                if (selectedUpload != null)
                {
                    //AutoMapper 
                    return _mapper.Map<UploadViewModel>(selectedUpload);
                }
                return null;
            }

            public IQueryable<UploadViewModel> GetAll()
            {
                var result = _db.Uploads
                     .OrderByDescending(u => u.DownloadCount)
                     .ProjectTo<UploadViewModel>(_mapper.ConfigurationProvider);
                return result;
            }

            public IQueryable<UploadViewModel> GetBy(string userId)
            {
                var result = _db.Uploads.Where(u => u.UserId == userId)
                    .OrderByDescending(u => u.UploadDate)
                   .ProjectTo<UploadViewModel>(_mapper.ConfigurationProvider);
                return result;
            }

            public async Task<int> GetUploadsCount()
            {
                return await _db.Uploads.CountAsync();
            }

            public async Task IncreamentDownloadCount(string id)
            {
                var selectedUpload = await _db.Uploads.FindAsync(id);
                if (selectedUpload != null)
                {
                    selectedUpload.DownloadCount++;

                    _db.Update(selectedUpload);
                    await _db.SaveChangesAsync();
                }
            }

            public IQueryable<UploadViewModel> Search(string term)
            {
                var result = _db.Uploads
                      .Where(u => u.OriginalFileName.Contains(term))
                      .OrderByDescending(u => u.DownloadCount)
                      .ProjectTo<UploadViewModel>(_mapper.ConfigurationProvider);
                return result;
            }
        }
    }


