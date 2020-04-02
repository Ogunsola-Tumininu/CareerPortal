using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using OfficeOpenXml;
using RecruitmentManagement.DAL;

namespace RecruitmentManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class VacanciesController : Controller
    {
        private Entities1 db = new Entities1();

        // GET: Vacancies
        public ActionResult Index()
        {
            return View(db.Vacancies.ToList());
        }

        [HttpPost]
        public ActionResult Upload(FormCollection formCollection)
        {
            var locationList = new List<Course>();
            if (Request != null)
            {
                HttpPostedFileBase file = Request.Files["UploadedFile"];

                if ((file == null) || (file.ContentLength == 0))
                {
                    ViewBag.ErrorMessage = "Please select a Excel in order to upload";
                    return View();
                }

                else
                {
                    if (file.FileName.EndsWith("xls") || file.FileName.EndsWith("xlsx"))
                    {
                        string fileName = file.FileName;
                        string fileContentType = file.ContentType;
                        byte[] fileBytes = new byte[file.ContentLength];
                        var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));

                        //Enter in Console "Install-Package EPPlus"
                        using (var package = new ExcelPackage(file.InputStream))
                        {
                            var currentSheet = package.Workbook.Worksheets;
                            var workSheet = currentSheet.First();
                            var noOfCol = workSheet.Dimension.End.Column;
                            var noOfRow = workSheet.Dimension.End.Row;
                            for (int rowIterator = 1; rowIterator <= noOfRow; rowIterator++)
                            {
                                if (rowIterator == 1)
                                {
                                    continue;
                                }
                                var course = new Course();
                                course.Name = workSheet.Cells[rowIterator, 1].Value.ToString();
                                //location.RegionId= Convert.ToInt32(workSheet.Cells[rowIterator, 2].Value);
                                //transaction.checkingAccountId = workSheet.Cells[rowIterator, 2].Value.ToString();
                                //transaction.Age = Convert.ToInt32(workSheet.Cells[rowIterator, 3].Value);
                                locationList.Add(course);
                            }
                        }
                        foreach (var item in locationList)
                        {
                            db.Courses.Add(item);
                        }
                        db.SaveChanges();

                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "The file extension must be excel ie xls or xlsx extension";
                        return View();
                    }

                }
            }

            return View();
        }

        // GET: Vacancies/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Vacancy vacancy = db.Vacancies.Find(id);
            if (vacancy == null)
            {
                return HttpNotFound();
            }
            var vacancyRequirements = db.Requirements.Where(vf => vf.VacancyId == id).ToList();
            var vacancyFunctions = db.VacancyFunctions.Where(vf => vf.VacancyId == id).ToList();
            ViewBag.VacancyRequirements = vacancyRequirements;
            ViewBag.VacancyFunctions = vacancyFunctions;
            return View(vacancy);
        }

        public ActionResult Location(int Id)
        {
            var locations = db.Locations.Where(l => l.RegionId == Id).ToList(); 
            return Json(locations, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult JobFunction(string jobFunction, int Id)
        {
            
            var funct = new VacancyFunction();
            funct.VacancyId = Id;
            funct.FunctionDescription = jobFunction;
            var locations = db.VacancyFunctions.Add(funct);
            db.SaveChanges();
            return Json("Vacancy Function saved successfully ", JsonRequestBehavior.AllowGet);
            //return RedirectToAction("index");
        }

        [HttpPost]
        public ActionResult JobRequirement(string jobRequirement, int Id)
        {

            var req = new Requirement();
            req.VacancyId = Id;
            req.RequirementDescription = jobRequirement;
            var newReq = db.Requirements.Add(req);
            db.SaveChanges();
            return Json("Vacancy Requirement saved successfully ", JsonRequestBehavior.AllowGet);
            //return RedirectToAction("index");
        }

        // GET: Vacancies/Create
        public ActionResult Create()
        {
            var jobDepartment = new SelectList(db.Departments.ToList(), "Id", "Name");
            ViewData["JobDepartment"] = jobDepartment;
            ViewBag.JobDepartment = new SelectList(db.Departments, "Id", "Name");

            var jobRegion = new SelectList(db.Regions, "Id", "Name");
            ViewData["JobRegion"] = jobRegion;
            ViewBag.JobRegion = new SelectList(db.Regions, "Id", "Name");


            return View();
        }

        // POST: Vacancies/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public JsonResult Create([Bind(Include = "Title,JobRegion,JobLocation,Department,EmploymentType,Duration,Level,Description,Qualification,Deadline")] Vacancy vacancy)
        {
            if (ModelState.IsValid)
            {
                var newVacancy = db.Vacancies.Add(vacancy);
          
                db.SaveChanges();
                return Json(new { Result = newVacancy.Id }, JsonRequestBehavior.AllowGet);
                //return RedirectToAction("Index");
            }

            return Json(new { error = true, result ="Could not save"}, JsonRequestBehavior.AllowGet);
        }

        // GET: Vacancies/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Vacancy vacancy = db.Vacancies.Find(id);
            if (vacancy == null)
            {
                return HttpNotFound();
            }
            ViewBag.JobDepartment = new SelectList(db.Departments, "Id", "Name");
            ViewBag.JobRegion = new SelectList(db.Regions, "Id", "Name");
            
            ViewBag.JobFunctions = db.VacancyFunctions.Where(vf => vf.VacancyId == id).ToList();
            ViewBag.Requirements = db.Requirements.Where(vf => vf.VacancyId == id).ToList();

            ViewBag.JobLocation = new SelectList(db.Locations, "Name", "Name");
            return View(vacancy);
        }

        // POST: Vacancies/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Title,JobRegion,JobLocation,Department,Duration,Level,Description,Qualification,Deadline,EmploymentType")] Vacancy vacancy)
        {
            if (ModelState.IsValid)
            {
                db.Entry(vacancy).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(vacancy);
        }

        [HttpPost]
        public ActionResult EditFunction(int id, string FunctionDescription)
        {
            var func = db.VacancyFunctions.Find(id);
            func.FunctionDescription = FunctionDescription;
            db.SaveChanges();
            return Json("Vacancy Function saved successfully", JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult EditRequirement(int id, string reqDescription)
        {
            var req = db.Requirements.Find(id);
            req.RequirementDescription = reqDescription;
            db.SaveChanges();
            return Json("Requirement saved successfully", JsonRequestBehavior.AllowGet);
        }

        public ActionResult ApplicantList(int VacancyId)
        {
            var appList = db.VacancyPostActivities.Where(v => v.VacancyId == VacancyId).ToList();
            return View(appList);
        }

        public ActionResult ApplicantProfile(string userId)
        {
            var appProfile = db.Profiles.Where(p => p.UserId == userId).FirstOrDefault();
            ViewBag.Experience = db.Experiences.Where(e => e.UserId == userId).ToList();
            ViewBag.Education = db.EducationDetails.Where(e => e.UserId == userId).ToList();
            ViewBag.Documents = db.DocumentPaths.Where(d => d.UserId == userId).ToList();
            return View(appProfile);
        }

        // GET: Vacancies/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Vacancy vacancy = db.Vacancies.Find(id);
            if (vacancy == null)
            {
                return HttpNotFound();
            }
            var vacancyFunctions = db.VacancyFunctions.Where(vf => vf.VacancyId == id).ToList();
            ViewBag.VacancyFunctions = vacancyFunctions;
            return View(vacancy);
        }

        // POST: Vacancies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Vacancy vacancy = db.Vacancies.Find(id);

            var vacancyFunc = db.VacancyFunctions.Where(f => f.VacancyId == id).ToList();
            var vacancyReq = db.Requirements.Where(f => f.VacancyId == id).ToList();
            db.Vacancies.Remove(vacancy);

            if (vacancyFunc.Count() > 0)
            {
                foreach (var func in vacancyFunc)
                {
                    db.VacancyFunctions.Remove(func);
                }
            }

            if (vacancyReq.Count() > 0)
            {
                foreach (var req in vacancyReq)
                {
                    db.Requirements.Remove(req);
                }
            }

            db.SaveChanges();
            return RedirectToAction("Index");
        }

        //[HttpPost]
        public ActionResult DownloadFile(string filePath, string fileName)
        {
            //string path = AppDomain.CurrentDomain.BaseDirectory + "Content/template/";
            //byte[] filebyte = System.IO.File.ReadAllBytes(path + "schedulesTemplate.xlsx");
            //string fileName = "schedulesTemplate.xlsx";
            byte[] filebyte = System.IO.File.ReadAllBytes(filePath);
            return File(filebyte, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
