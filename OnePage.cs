//Make Model:
using static System.Collections.Specialized.BitVector32;
using System;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

** public class Product
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int PId { get; set; }
    public String PName { get; set; }
    public int Price { get; set; }
    public bool IsAviable { get; set; }
    public DateTime Pdate { get; set; }
    public string Image { get; set; }
    public virtual IList<Details> Details { get; set; }
}
** public class Color
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int CId { get; set; }
    public string CName { get; set; }
    public virtual IList<Details> Details { get; set; }
** public class Details
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DId { get; set; }
        [ForeignKey("Product")]
        public int PId { get; set; }
        [ForeignKey("Color")]
        public int CId { get; set; }
        public virtual Product Product { get; set; }
        public virtual Color Color { get; set; }
    }
** public class ProductDbContext : DbContext
    {
        public ProductDbContext() : base("ProductDb34")
        {

        }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<Color> Colors { get; set; }
        public virtual DbSet<Details> Details { get; set; }
------- 
//Install-Package EntityFramework
//Enable-Migrations
//Add-Migration
//Update-Database
//Install-Package PagedList.Mvc
--------

//In Vm Folder  ProductVm.cs
  ** public class ProductVm
        {
            public ProductVm()
            {
                this.Details = new List<Details>();
            }
            public int PId { get; set; }
            [Required, DisplayName("Product Name")]
            public String PName { get; set; }
            public int Price { get; set; }
            public bool IsAviable { get; set; }
            public DateTime Pdate { get; set; }
            public string Image { get; set; }
            public HttpPostedFileBase ImageFile { get; set; }
            public virtual List<Details> Details { get; set; }
        }
// Controller:
    ProductDbContext db = new ProductDbContext();
        public ActionResult Index(int? page)
        {
            int pageSize = 2;
            int pageNumber = page ?? 1;
            var products = db.Products.OrderByDescending(p => p.PId).ToPagedList(pageNumber, pageSize);
            foreach (var product in products)
            {
                db.Entry(product).Collection(p => p.Details).Load();
                foreach (var detail in product.Details)
                {
                    db.Entry(detail).Reference(d => d.Color).Load();
                }
            }

            return View(products);
        }
        [HttpGet]
        public ActionResult Create()
        {
            return PartialView("Create");
        }
        [HttpPost]
        public ActionResult Create(ProductVm productVm, int[] CId)
        {
            if (ModelState.IsValid)
            {
                var product = new Product()
                {
                    PName = productVm.PName,
                    IsAviable = productVm.IsAviable,
                    Pdate = productVm.Pdate,
                    Price = productVm.Price,
                };
                HttpPostedFileBase file = productVm.ImageFile;
                if (file != null)
                {
                    string filename = Path.Combine("/Images/", DateTime.Now.Ticks.ToString() + Path.GetExtension(file.FileName));
                    file.SaveAs(Server.MapPath(filename));
                    product.Image = filename;
                }
                foreach (var i in CId)
                {
                    var d = new Details()
                    {
                        Product = product,
                        PId = product.PId,
                        CId = i
                    };
                    db.Details.Add(d);
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(productVm);
        }
        public ActionResult AddColor(int? id)
        {
            ViewBag.Color = new SelectList(db.Colors.ToList(), "CId", "CName", id ?? 0);
            return PartialView("AddColor");
        }
        [HttpGet]
        public ActionResult Edit(int? id)
        {
            var product = db.Products.Find(id);
            var details = db.Details.Where(e => e.PId == product.PId).ToList();
            var pobj = new ProductVm()
            {
                PId = product.PId,
                Details = details,
                Image = product.Image,
                IsAviable = product.IsAviable,
                Pdate = product.Pdate,
                PName = product.PName,
                Price = product.Price,
            };
            return PartialView("Edit", pobj);
        }
        [HttpPost]
        public ActionResult Edit(ProductVm productVm, int[] CId)
        {
            if (ModelState.IsValid)
            {
                var product = db.Products.Find(productVm.PId);
                var details = db.Details.Where(e => e.PId == product.PId).ToList();
                product.PName = productVm.PName;
                product.IsAviable = productVm.IsAviable;
                product.Pdate = productVm.Pdate;
                product.Price = productVm.Price;

                HttpPostedFileBase file = productVm.ImageFile;
                if (file != null)
                {
                    string filename = Path.Combine("/Images/", DateTime.Now.Ticks.ToString() + Path.GetExtension(file.FileName));
                    file.SaveAs(Server.MapPath(filename));
                    product.Image = filename;
                }
                else
                {
                    product.Image = product.Image;
                }
                db.Details.RemoveRange(details);
                foreach (var i in CId)
                {
                    var d = new Details()
                    {
                        Product = product,
                        PId = product.PId,
                        CId = i
                    };
                    db.Details.Add(d);
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(productVm);
        }
        public ActionResult Delete(int? id)
        {
            var product = db.Products.Find(id);
            var details = db.Details.Where(e => e.PId == product.PId).ToList();
            db.Details.RemoveRange(details);
            db.Products.Remove(product);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }

    //Index page:
    @using PagedList.Mvc;
    @model PagedList.IPagedList<WebApp1.Models.Product>


    @Html.ActionLink("Create", "Create", null, new { @class = "btn btn-primary crBtn my-3 px-4" })
< table class= "table table-bordered" >
    < thead >
        < tr >
            < th > Product Id </ th >
            < th > Product Name </ th >
            < th > Price </ th >
            < th > Date </ th >
            < th > Image </ th >
            < th > Is Aviable </ th >
            < th > Action </ th >
        </ tr >
    </ thead >
    < tbody >
        @foreach(var product in Model)
        {
            < tr >
                < td > @product.PId </ td >
                < td > @product.PName </ td >
                < td > @product.Price </ td >
                < td > @product.Pdate </ td >
                < td >< img src = "@product.Image" width = "50" height = "50" /></ td >
                < td > @product.IsAviable </ td >
                < td >
                    < button class= "btn btn-warning edBtn" type = "button" data - id = "@product.PId" > Edit </ button >
                    @Html.ActionLink("Delete", "Delete", null, new { id = product.PId }, new { @class = "btn btn-danger" })
                </ td >
            </ tr >
            < tr >
                < td colspan = "13" >
                    < table class= "table table-active text-center" >
                        < thead >
                            < tr >
                                < th > Color Id </ th >
                                < th > Color Name </ th >
                            </ tr >
                        </ thead >
                        < tbody >
                            @foreach(var i in product.Details)
                            {
                                < tr >
                                    < td > @i.Color.CId </ td >
                                    < td > @i.Color.CName </ td >
                                </ tr >
                            }
                        </ tbody >
                    </ table >
                </ td >
            </ tr >
        }
    </ tbody >
</ table >
< section class= "mcon" style = "display:none;" >
    < div class= "mbod p-5" >
        < div class= "macon" ></ div >
        < hr />
        < div class= "modal-footer" >
            < button class= "btn btn-danger clbtn" > Close </ button >
        </ div >
    </ div >
</ section >
< section class= "d-flex justify-content-center" id = "pcon" >
    < div class= "pagination" >
        @Html.PagedListPager(Model, page => Url.Action("Index", new { page }))
    </ div >
</ section >


//Create:

@model WebApp1.Models.vm.ProductVm

@using(Ajax.BeginForm("Create", "Product", null, new AjaxOptions { HttpMethod = "POST", OnSuccess = "", OnFailure = "" }, new { enctype = "multipart/form-data" }))
{
    @Html.AntiForgeryToken()

    < div class= "form-horizontal" >
        < h4 > Add Product </ h4 >
        < hr />
        @Html.ValidationSummary(true, "", new { @class = "text-danger" })

        < div class= "form-group" >
            @Html.LabelFor(model => model.PName, htmlAttributes: new { @class = "control-label col-md-2" })
            < div class= "col-md-10" >
                @Html.EditorFor(model => model.PName, new { htmlAttributes = new { @class = "form-control" } })
                @Html.ValidationMessageFor(model => model.PName, "", new { @class = "text-danger" })
            </ div >
        </ div >

        < div class= "form-group" >
            @Html.LabelFor(model => model.Price, htmlAttributes: new { @class = "control-label col-md-2" })
            < div class= "col-md-10" >
                @Html.EditorFor(model => model.Price, new { htmlAttributes = new { @class = "form-control" } })
            </ div >
        </ div >

        < div class= "form-group" >
            @Html.LabelFor(model => model.IsAviable, htmlAttributes: new { @class = "control-label col-md-2" })
            < div class= "col-md-10" >
                < div class= "checkbox" >
                    @Html.CheckBoxFor(model => model.IsAviable)
                </ div >
            </ div >
        </ div >

        < div class= "form-group" >
            @Html.LabelFor(model => model.Pdate, htmlAttributes: new { @class = "control-label col-md-2" })
            < div class= "col-md-10" >
                @Html.TextBoxFor(model => model.Pdate, new { @class = "form-control", type = "date" })
            </ div >
        </ div >

        < div class= "form-group" >
            @Html.LabelFor(model => model.Image, htmlAttributes: new { @class = "control-label col-md-2" })
            < div class= "col-md-10" >
                @Html.TextBoxFor(model => model.ImageFile, new { @class = "form-control", type = "file" })
            </ div >
        </ div >

        < div class= "my-3" >
            < div >
                @Html.ActionLink("Add More", "", null, new { @class = "btn btn-info addmore" })
            </ div >
            < div class= "con" >
                @Html.Action("AddColor")
            </ div >
        </ div >

        < div class= "form-group" >
            < div class= "col-md-offset-2 col-md-10" >
                < input type = "submit" value = "Create" class= "btn btn-primary" />
            </ div >
        </ div >
    </ div >
}

< script type = "text/javascript" >
    $(".addmore").click((e) => {
e.preventDefault();
        $.ajax({
url: "/Product/AddColor",
            type: "get",
            success: (d) => {
                $(".con").append(d);
            }
        });
    });
</ script >


//Edite:


@model WebApp1.Models.vm.ProductVm

@using(Html.BeginForm("Edit", "Product", FormMethod.Post, new { enctype = "multipart/form-data" }))
{
    @Html.AntiForgeryToken()

    < div class= "form-horizontal" >
        < h4 > Edit Product </ h4 >
        < hr />
        @Html.ValidationSummary(true, "", new { @class = "text-danger" })
        @Html.HiddenFor(model => model.PId)

        < div class= "form-group" >
            @Html.LabelFor(model => model.PName, htmlAttributes: new { @class = "control-label col-md-2" })
            < div class= "col-md-10" >
                @Html.EditorFor(model => model.PName, new { htmlAttributes = new { @class = "form-control" } })
                @Html.ValidationMessageFor(model => model.PName, "", new { @class = "text-danger" })
            </ div >
        </ div >

        < div class= "form-group" >
            @Html.LabelFor(model => model.Price, htmlAttributes: new { @class = "control-label col-md-2" })
            < div class= "col-md-10" >
                @Html.EditorFor(model => model.Price, new { htmlAttributes = new { @class = "form-control" } })
            </ div >
        </ div >

        < div class= "form-group" >
            @Html.LabelFor(model => model.IsAviable, htmlAttributes: new { @class = "control-label col-md-2" })
            < div class= "col-md-10" >
                < div class= "checkbox" >
                    @Html.CheckBoxFor(model => model.IsAviable)
                </ div >
            </ div >
        </ div >

        < div class= "form-group" >
            @Html.LabelFor(model => model.Pdate, htmlAttributes: new { @class = "control-label col-md-2" })
            < div class= "col-md-10" >
                @Html.TextBox("Pdate", Model.Pdate.ToString("yyyy-MM-dd"), new { @class = "form-control", type = "date" })
            </ div >
        </ div >

        < div class= "form-group" >
            @Html.LabelFor(model => model.Image, htmlAttributes: new { @class = "control-label col-md-2" })
            < div class= "col-md-10" >
                @Html.TextBoxFor(model => model.ImageFile, new { @class = "form-control", type = "file" })
                < img src = "@Model.Image" width = "50" height = "50" />
            </ div >
        </ div >

        < div class= "my-3" >
            < div >
                @Html.ActionLink("Add More", "", null, new { @class = "btn btn-info addmore" })
            </ div >
            < div class= "con" >
                @foreach(var i in Model.Details)
                {
    @Html.Action("AddColor", "", new { id = i.Color.CId })
                }
            </ div >
        </ div >

        < div class= "form-group" >
            < div class= "col-md-offset-2 col-md-10" >
                < input type = "submit" value = "Save" class= "btn btn-success" />
            </ div >
        </ div >
    </ div >
}

< script type = "text/javascript" >
    $(".addmore").click((e) => {
e.preventDefault();
        $.ajax({
url: "/Product/AddColor",
            type: "get",
            success: (d) => {
                $(".con").append(d);
            }
        });
    });
</ script >


//AddAjex  AddColor:

@model WebApp1.Models.Color

< div class= "rc d-flex my-3" >
    < div class= "me-2" >
        @Html.DropDownListFor(c => c.CId, ViewBag.Color as SelectList, "---select Color---", new { @class = "form-control" })
    </ div >
    < div >
        @Html.ActionLink("Remove", "", null, new { @class = "btn btn-danger remove" })
    </ div >
</ div >


//_Layout:
< !DOCTYPE html >
< html >
< head >
    < meta charset = "utf-8" />
    < meta name = "viewport" content = "width=device-width, initial-scale=1.0" >
    < title > @ViewBag.Title - My ASP.NET Application</title>
    <link href = "~/Content/Site.css" rel= "stylesheet" type= "text/css" />
    < link href= "~/Content/bootstrap.min.css" rel= "stylesheet" type= "text/css" />
    < script src= "~/Scripts/modernizr-2.6.2.js" ></ script >
    < script src= "~/Scripts/bootstrap.min.js" ></ script >
    < script src= "~/Scripts/jquery-3.7.1.min.js" ></ script >
    < link href= "~/Content/Site.css" rel= "stylesheet" />
    < link href= "~/Content/PagedList.css" rel= "stylesheet" />
    @Styles.Render("~/Content/css")
    @Scripts.Render("~/bundles/modernizr")
</ head >
< body >
    < nav class= "navbar navbar-expand-sm navbar-toggleable-sm navbar-dark bg-dark" >
        < div class= "container" >
            @Html.ActionLink("Product Hosuse", "Index", "Home", new { area = "" }, new { @class = "navbar-brand" })
            < button type = "button" class= "navbar-toggler" data - bs - toggle = "collapse" data - bs - target = ".navbar-collapse" title = "Toggle navigation" aria - controls = "navbarSupportedContent"
                    aria - expanded = "false" aria - label = "Toggle navigation" >
                < span class= "navbar-toggler-icon" ></ span >
            </ button >
            < div class= "collapse navbar-collapse d-sm-inline-flex justify-content-between" >
                < ul class= "navbar-nav flex-grow-1" >
                    < li > @Html.ActionLink("Product", "Index", "Product", new { area = "" }, new { @class = "nav-link" }) </ li >
                </ ul >
            </ div >
        </ div >
    </ nav >
    < div class= "container body-content" >
        @RenderBody()
        < hr />
        < footer >
            < p > &copy; @DateTime.Now.Year - My ASP.NET Application</p>
        </footer>
    </div>

    @Scripts.Render("~/bundles/jquery")
    @Scripts.Render("~/bundles/bootstrap")
    @RenderSection("scripts", required: false)
    < script src = "~/Scripts/jquery-1.10.2.min.js" ></ script >
    < script src = "~/Scripts/bootstrap.min.js" ></ script >
    < script type = "text/javascript" >
    $(document).ready(() => {
        $(".crBtn").click((e) => {
         e.preventDefault();
            $("#pcon").hide();
            $.ajax({
         url: "/Product/Create",
                type: "get",
                success: (d) => {
                    $(".macon").html(d);
                    $(".mcon").show();
                }
            });
});

        $(".edBtn").click((e) => {
         e.preventDefault();
         var id = $(e.currentTarget).data("id");
            $("#pcon").hide();
            $.ajax({
         url: "/Product/Edit/" + id,
                type: "get",
                success: (d) => {
                    $(".macon").html(d);
                    $(".mcon").show();
                }
            });
        });

        $(document).on("click", ".remove", (e) => {
             e.preventDefault();
            $(e.currentTarget).closest(".rc").remove();
         });

        $(".clbtn").click(() => {
            $(".mcon").hide();
            $(".pcon").show();
         });
    });
    </ script >

</ body >
</ html >


//SiteBootsp:
.mcon {
width: 100vw;
height: 100vh;
    background - color: rgba(0, 0, 0, .2);
position: fixed;
    top: 0;
left: 0;
display: flex;
    justify - content: center;
    align - items: center;
    z - index: 1000;
}

.mbod {
    width: 50 %;
max - height: 80vh;
background - color: #fff;
    overflow: scroll;
scrollbar - width: none;
}

