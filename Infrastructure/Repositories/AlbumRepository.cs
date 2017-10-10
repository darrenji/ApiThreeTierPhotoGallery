using ApiThreeTierPhotoGallery.Entities;
using ApiThreeTierPhotoGallery.Infrastructure.Repositories.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiThreeTierPhotoGallery.Infrastructure.Repositories
{
    public class AlbumRepository : EntityBaseRepository<Album>, IAlbumRepository
    {
        public AlbumRepository(PhotoGalleryContext context)
            : base(context)
        { }
    }
}
