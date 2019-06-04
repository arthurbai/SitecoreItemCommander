﻿namespace ItemCommander.EntityService.Controllers
{
    using ItemCommander.EntityService.Interfaces;
    using ItemCommander.EntityService.Models;
    using ItemCommander.EntityService.Repositories;
    using Sitecore;
    using Sitecore.Configuration;
    using Sitecore.Data;
    using Sitecore.Data.Items;
    using Sitecore.Install;
    using Sitecore.Install.Framework;
    using Sitecore.Install.Items;
    using Sitecore.Install.Zip;
    using Sitecore.Security.Accounts;
    using Sitecore.Services.Core;
    using Sitecore.Services.Infrastructure.Sitecore.Services;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Web;
    using System.Web.Http;
    using System.Web.Http.Cors;

    /// <summary>
    /// Entity controller for item commander
    /// </summary>
    /// <seealso cref="Sitecore.Services.Infrastructure.Sitecore.Services.EntityService{ItemCommander.EntityService.Models.ItemResponse}" />
    [ServicesController]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class EntityController : EntityService<ItemResponse>
    {
        /// <summary>
        /// The custom repository actions
        /// </summary>
        private IGenericItemRepository<ItemResponse> _customRepositoryActions;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityController"/> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        public EntityController(IGenericItemRepository<ItemResponse> repository)
            : base(repository)
        {
            _customRepositoryActions = repository;
        }

        //SampleUR: /sitecore/api/ssc/ItemCommander-EntityService-Controllers/Entity/{id}/{actionName}?{queryStrings}
        //SampleUR: /sitecore/api/ssc/ItemCommander-EntityService-Controllers/Entity/{80FDC514-CD6A-4EEF-B9C2-6CFA82B0F37A}/findbyid?language=en-gb

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityController"/> class.
        /// </summary>
        public EntityController()
            : this(new ItemRepository())
        {
        }

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="db">The database.</param>
        /// <returns>Children</returns>
        [HttpGet]
        [ActionName("Children")]
        public ItemCommanderResponse GetChildren(string id, string db)
        {
            return this._customRepositoryActions.GetChildren(id, db);
        }

        /// <summary>
        /// Fasts the v iew.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="db">The database.</param>
        /// <returns>fast view contract</returns>
        [HttpGet]
        [ActionName("fastview")]
        public FastViewResponse FastVIew(string id, string db)
        {
            return this._customRepositoryActions.GetFastView(id, db);
        }

        /// <summary>
        /// Queries the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="query">The query.</param>
        /// <param name="db">The database.</param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("copy")]
        public IHttpActionResult Query(string id, CopyRequest query, string db)
        {
            this._customRepositoryActions.Copy(query, db);
            return this.Ok();
        }

        /// <summary>
        /// Copies the single.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="query">The query.</param>
        /// <param name="db">The database.</param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("copysingle")]
        public IHttpActionResult CopySingle(string id, CopySingle query, string db)
        {
            this._customRepositoryActions.CopySingle(query, db);
            return this.Ok();
        }

        [HttpPost]
        [ActionName("move")]
        public IHttpActionResult Move(string id, MoveRequest query, string db)
        {
            this._customRepositoryActions.Move(query, db);
            return this.Ok();
        }

        [HttpPost]
        [ActionName("delete")]
        public IHttpActionResult Delete(string id, DeleteRequest query, string db)
        {
            this._customRepositoryActions.Delete(query, db);
            return this.Ok();
        }

        [HttpPost]
        [ActionName("lock")]
        public IHttpActionResult Lock(string id, LockRequest query, string db)
        {
            this._customRepositoryActions.Lock(query, db);
            return this.Ok();
        }

        [HttpPost]
        [ActionName("folder")]
        public IHttpActionResult folder(string id, CreateItemRequest query, string db)
        {
            this._customRepositoryActions.CreateItem(query, db);
            return this.Ok();
        }

        [HttpGet]
        [ActionName("search")]
        public ItemCommanderResponse search(string id, string keyword, string db)
        {
            return this._customRepositoryActions.Search(keyword, db);
        }

        [HttpGet]
        [ActionName("insertoptions")]
        public List<ItemResponse> InsertOptions(string id, string db)
        {
            return this._customRepositoryActions.GetInsertOptions(id, db);
        }

        [HttpGet]
        [ActionName("mediaurl")]
        public DownloadResponse MediaUrl(string id, string db)
        {
            return new DownloadResponse { FileName = _customRepositoryActions.GetMediaUrl(id, db) };
        }

        [HttpPost]
        [ActionName("package")]
        public DownloadResponse Package(string id, DeleteRequest request, string db)
        {
            var file = this.GeneratePackage(this._customRepositoryActions.GetItems(request.Items, db));

            return new DownloadResponse { FileName = Path.GetFileName(file) };
        }

        [HttpPost]
        [ActionName("GetItems")]
        public List<ItemResponse> GetItem(string id, GetItemRequest request, string db)
        {
            var ids = request.RawValue.Split('|');

            List<Item> items = new List<Item>(); ;

            if (!string.IsNullOrEmpty(request.RawValue) && ids.Any())
            {
                items = this._customRepositoryActions.GetItems(ids.ToList(), db);
            }

            return items.Select(t => new ItemResponse
            {
                Name = t.Name,
                Id = t.ID.ToString(),
                Language = t.Language.ToString(),
                Path = t.Paths.FullPath,
                TemplateName = t.TemplateName,
                Fields = new List<FieldResponse>(),
                HasChildren = t.HasChildren,
                LastModified = t.Statistics.Updated,
                Created = t.Statistics.Created,
                Icon = ItemRepository.GetIcon(t),
                IsLocked = t.Locking.IsLocked(),
                IsHidden = t["__Hidden"] == "1"
            }).ToList();
        }

        [HttpGet]
        [ActionName("download")]
        public HttpResponseMessage Package(string id, string fileName)
        {
            var file = HttpContext.Current.Server.MapPath(Path.Combine(Settings.PackagePath, fileName));
            var fileExtenstion = Path.GetExtension(file);

            var dataBytes = File.ReadAllBytes(file);
            var dataStream = new MemoryStream(dataBytes);

            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new StreamContent(dataStream);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = fileName
            };

            return result;
        }

        [HttpGet]
        [ActionName("IsOk")]
        [Obsolete]
        public string IsOk()
        {
            return "Ok";
        }

        private string GeneratePackage(List<Item> list)
        {
            string fullName = Context.User.Profile.FullName;
            using (new UserSwitcher(Sitecore.Security.Accounts.User.FromName("sitecore\\admin", false)))
            {
                Database contentDatabase = Context.ContentDatabase;
                PackageProject solution = new PackageProject();
                solution.Metadata.PackageName = list[0].Name;
                solution.Metadata.Author = fullName;
                ExplicitItemSource explicitItemSource = new ExplicitItemSource();
                explicitItemSource.Name = list[0].Name;

                Item[] objArray = list.ToArray();
                if (objArray != null && objArray.Length > 0)
                    list.AddRange((IEnumerable<Item>)objArray);
                foreach (Item obj in list)
                {
                    explicitItemSource.Entries.Add(new ItemReference(obj.Uri, false).ToString());
                }
                solution.Sources.Add((ISource<PackageEntry>)explicitItemSource);
                solution.SaveProject = true;

                string fileName = list[0].Name + "_" + DateTime.Now.ToString("dd_MM_yyyy_hh_mm_ss_fffffff") + ".zip";
                string str = Settings.PackagePath + "\\";

                using (PackageWriter packageWriter = new PackageWriter(str + fileName))
                {
                    Context.SetActiveSite("shell");
                    ((BaseSink<PackageEntry>)packageWriter).Initialize(Installer.CreateInstallationContext());
                    PackageGenerator.GeneratePackage(solution, (ISink<PackageEntry>)packageWriter);
                    Context.SetActiveSite("website");
                }

                return str + fileName;
            }
        }
    }
}