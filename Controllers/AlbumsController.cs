using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ApiThreeTierPhotoGallery.Infrastructure.Repositories.Abstract;
using ApiThreeTierPhotoGallery.Infrastructure.Core;
using ApiThreeTierPhotoGallery.ViewModels;
using ApiThreeTierPhotoGallery.Entities;
using AutoMapper;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace ApiThreeTierPhotoGallery.Controllers
{
    [Route("api/[controller]")]
    public class AlbumsController : Controller
    {

        private readonly IAuthorizationService _authorizationService;
        IAlbumRepository _albumRepository;
        ILoggingRepository _loggingRepository;

        public AlbumsController(IAuthorizationService authorizationService,
                                IAlbumRepository albumRepository,
                                ILoggingRepository loggingRepository)
        {
            _authorizationService = authorizationService;
            _albumRepository = albumRepository;
            _loggingRepository = loggingRepository;
        }

        /// <summary>
        /// 所有相册下的所有图片
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [Authorize(Policy = "AdminOnly")]
        [HttpGet("{page:int=0}/{pageSize=12}")]
        public async Task<IActionResult> Get(int? page, int? pageSize)
        {
            PaginationSet<AlbumViewModel> pagedSet = new PaginationSet<AlbumViewModel>();

            try
            {
                if(await _authorizationService.AuthorizeAsync(User, "AdminOnly"))
                {
                    int currentPage = page.Value;
                    int currentPageSize = pageSize.Value;

                    List<Album> _albums = null;

                    //记录Album的总数量
                    int _totalAlbums = new int();

                    _albums = _albumRepository
                        .AllIncluding(t => t.Photos)
                        .OrderBy(t => t.Id)
                        .Skip(currentPage * currentPageSize)
                        .Take(currentPageSize)
                        .ToList();

                    _totalAlbums = _albumRepository.GetAll().Count();

                    //转换成ViewModel
                    IEnumerable<AlbumViewModel> _albumsVM = Mapper.Map<IEnumerable<Album>, IEnumerable<AlbumViewModel>>(_albums);

                    //转换成分页
                    pagedSet = new PaginationSet<AlbumViewModel>() {
                        Page = currentPage,
                        TotalCount = _totalAlbums,
                        TotalPages = (int)Math.Ceiling((decimal)_totalAlbums / currentPageSize),
                        Items = _albumsVM
                    };
                }
                else
                {
                    CodeResultStatus _codeResult = new CodeResultStatus(401);
                    return new ObjectResult(_codeResult);
                }
            }
            catch (Exception ex)
            {

                _loggingRepository.Add(new Error() {
                    Message = ex.Message, 
                    StackTrace = ex.StackTrace,
                    DateCreated=DateTime.Now
                });

                _loggingRepository.Commit();
            }

            return new ObjectResult(pagedSet);
        }


        /// <summary>
        /// 某个相册下的所有图片
        /// </summary>
        /// <param name="id"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [Authorize(Policy ="AdminOnly")]
        [HttpGet("{id:int}/photos/{page:int=0}/{pageSize=12}")]
        public PaginationSet<PhotoViewModel> Get(int id, int? page, int? pageSize)
        {
            PaginationSet<PhotoViewModel> pagedSet = null;

            try
            {
                int currentPage = page.Value;
                int currentPageSize = pageSize.Value;

                List<Photo> _photos = null;
                int _totalPhotos = new int();

                Album _album = _albumRepository.GetSingle(t => t.Id == id, t => t.Photos);

                _photos = _album
                    .Photos
                    .OrderBy(t => t.Id)
                    .Skip(currentPage * currentPageSize)
                    .Take(currentPageSize)
                    .ToList();

                _totalPhotos = _album.Photos.Count();

                //转换成View Model
                IEnumerable<PhotoViewModel> _photoVM = Mapper.Map<IEnumerable<Photo>, IEnumerable<PhotoViewModel>>(_photos);

                pagedSet = new PaginationSet<PhotoViewModel>() {
                    Page = currentPage,
                    TotalCount = _totalPhotos,
                    TotalPages = (int)Math.Ceiling((decimal)_totalPhotos / currentPageSize),
                    Items = _photoVM
                };
            }
            catch (Exception ex)
            {

                _loggingRepository.Add(new Error() {
                    Message = ex.Message,
                    StackTrace = ex.StackTrace,
                    DateCreated = DateTime.Now
                });

                _loggingRepository.Commit();
            }

            return pagedSet;
        }
    }
}
