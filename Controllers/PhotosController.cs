using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ApiThreeTierPhotoGallery.Infrastructure.Repositories.Abstract;
using ApiThreeTierPhotoGallery.ViewModels;
using ApiThreeTierPhotoGallery.Infrastructure.Core;
using ApiThreeTierPhotoGallery.Entities;
using AutoMapper;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace ApiThreeTierPhotoGallery.Controllers
{
    [Route("api/[controller]")]
    public class PhotosController : Controller
    {
        IPhotoRepository _photoRepository;
        ILoggingRepository _loggingRepository;

        public PhotosController(IPhotoRepository photoRepository, ILoggingRepository loggingRepository)
        {
            _photoRepository = photoRepository;
            _loggingRepository = loggingRepository;
        }
        

        [HttpGet("{page:int=0}/{pageSize=12}")]
        public PaginationSet<PhotoViewModel> Get(int? page, int? pageSize)
        {
            PaginationSet<PhotoViewModel> pagedSet = null;

            try
            {
                int currentPage = page.Value;
                int currentPageSize = pageSize.Value;

                List<Photo> _photos = null;
                int _totalPhotosCount = new int();

                _photos = _photoRepository
                    .AllIncluding(t => t.Album)
                    .OrderBy(t => t.Id)
                    .Skip(currentPage * currentPageSize)
                    .Take(currentPageSize)
                    .Take(currentPageSize)
                    .ToList();

                _totalPhotosCount = _photoRepository.GetAll().Count();

                //准备ViewModel
                IEnumerable<PhotoViewModel> _photosVM = Mapper.Map<IEnumerable<Photo>, IEnumerable<PhotoViewModel>>(_photos);

                //准备为分页的模型
                pagedSet = new PaginationSet<PhotoViewModel>()
                {
                    Page = currentPage,
                    TotalCount = _totalPhotosCount,
                    TotalPages= (int)Math.Ceiling((decimal)_totalPhotosCount / currentPageSize),
                    Items=_photosVM
                };
            }
            catch (Exception ex)
            {
                _loggingRepository.Add(new Error() {
                    Message = ex.Message,
                    StackTrace = ex.StackTrace,
                    DateCreated =DateTime.Now
                });

                _loggingRepository.Commit();
            }

            return pagedSet;
        }


        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            IActionResult _result = new ObjectResult(false);
            GenericResult _removeResult = null;

            try
            {
                Photo _photoToRemove = this._photoRepository.GetSingle(id);
                _photoRepository.Delete(_photoToRemove);
                _photoRepository.Commit();

                _removeResult = new GenericResult() {
                    Succeeded = true,
                    Message = "Photo removed."
                };
            }
            catch (Exception ex)
            {

                _removeResult = new GenericResult() {
                    Succeeded = false,
                    Message = ex.Message
                };

                _loggingRepository.Add(new Error() {
                    Message = ex.Message, StackTrace=ex.StackTrace, DateCreated = DateTime.Now
                });
                _loggingRepository.Commit();
            }

            _result = new ObjectResult(_removeResult);
            return _result;
        }
    }
}
