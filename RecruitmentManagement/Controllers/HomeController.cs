using Microsoft.AspNet.Identity;
using RecruitmentManagement.DAL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RecruitmentManagement.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private Entities1 db = new Entities1();

        [AllowAnonymous]
        public ActionResult Index(string Region, string Dept,string Emptype, string Level )
        {
            var vacancies = db.Vacancies.OrderByDescending(v => v.Deadline).ToList();
            if(Region != null)
            {
                vacancies = vacancies.Where(v => v.JobRegion == Region).ToList();
            }
            if (Dept != null)
            {
                vacancies = vacancies.Where(v => v.Department == Dept).ToList();
            }
            if (Emptype != null)
            {
                vacancies = vacancies.Where(v => v.EmploymentType == Emptype).ToList();
            }
            if (Level != null)
            {
                vacancies = vacancies.Where(v => v.Level == Level).ToList();
            }
            return View(vacancies);
        }

        public ActionResult Vacancy(int Id)
        {
            var vacancy = db.Vacancies.Find(Id);
            var vacancyFunctions = db.VacancyFunctions.Where(vf => vf.VacancyId == Id).ToList();
            var vacancyRequirements = db.Requirements.Where(vf => vf.VacancyId == Id).ToList();
            ViewBag.VacancyFunctions = vacancyFunctions;
            ViewBag.VacancyRequirements = vacancyRequirements;
            return View(vacancy);
        }

        public ActionResult UserProfile(int VacancyId)
        {
            var userId = User.Identity.GetUserId();
            var user = db.AspNetUsers.Find(userId);

            var userProfile = db.Profiles.Where(p => p.UserId == userId).FirstOrDefault();
            if (userProfile != null)
            {
                ViewBag.Education = db.EducationDetails.Where(e => e.UserId == userId).ToList();
                ViewBag.Experience = db.Experiences.Where(e => e.UserId == userId).ToList();
            }
            ViewBag.User = user;
            ViewBag.VacancyId = VacancyId;

            ViewBag.States = new SelectList(db.Locations.OrderBy(s => s.Name), "Name", "Name");
            ViewBag.Countries = new SelectList(db.Countries.OrderBy(s => s.Name), "Name", "Name");
            ViewBag.University = new SelectList(db.Universities.OrderBy(s => s.Name), "Name", "Name");
            ViewBag.Courses = new SelectList(db.Courses.OrderBy(s => s.Name), "Name", "Name");
            var year = DateTime.Now.Year; 
           
            ViewBag.Year = Enumerable.Range(1900, (year-1899));
            return View(userProfile);
        }

        [HttpPost]
        public ActionResult UserProfile(int VacancyId, Profile profile)
        {
            var userId = User.Identity.GetUserId();
            var user = db.AspNetUsers.Find(userId);
            var vacancy = db.Vacancies.Find(VacancyId);

            if (ModelState.IsValid)
            { 
                profile.UserId = userId;

                var userProfile = db.Profiles.Where(p => p.UserId == userId).FirstOrDefault();
                if(userProfile != null)
                {
                    userProfile = profile;
                }
                else
                {
                    db.Profiles.Add(profile);
                }
                

                var apply = new VacancyPostActivity();
                apply.UserId = userId;
                apply.VacancyId = VacancyId;
                db.VacancyPostActivities.Add(apply);
                db.SaveChanges();
                var msg = PalLibrary.Messaging.SendEmail(profile.email, "hr@palpensions.com", "Application Successfully", $@"

                    <p>Hello {profile.FirstName} {profile.Surname},
 
                    <p>Your application for {vacancy.Title} position at PAL PENSION was successfull </p><br />
                    
                    <p>We will get back to if you are shortlisted for the position</p><br />
                    <hr />   
                     
                    <p>For a list of feedback,request or complaints, please visit https://pallite.palpensions.com/Staff/SupportLogs </p> 
   
                    <p>Regards<br/>
 
                    PAL Pensions</p>");
                return RedirectToAction("Successmessage");
            }

            ViewBag.States = new SelectList(db.Locations.OrderBy(s => s.Name), "Name", "Name");
            ViewBag.Countries = new SelectList(db.Countries.OrderBy(s => s.Name), "Name", "Name");
            ViewBag.University = new SelectList(db.Universities.OrderBy(s => s.Name), "Name", "Name");
            ViewBag.Courses = new SelectList(db.Courses.OrderBy(s => s.Name), "Name", "Name");
            var year = DateTime.Now.Year;

            ViewBag.Year = Enumerable.Range(1900, (year - 1899));
            return View();
        }

        [HttpPost]
        public ActionResult UploadFiles(FormCollection formCollection)
        {

            var userId = User.Identity.GetUserId();

            HttpPostedFileBase cvFile = Request.Files[0];
            //file upload process
            string fileType = formCollection["FileType"];
            
            var cvFileExt = Path.GetExtension(cvFile.FileName);
            var cvName = Path.GetFileName(cvFile.FileName);
            int MaxContentLength = 1024 * 1024; //1 MB
            string[] AllowedFileExtensions = new string[] { ".doc", ".docx", ".jpg", ".png", "gif", ".pdf" };

            if (cvFile.ContentLength > MaxContentLength)
            {
                //ViewBag.ErrorMessage = "Your file is too large, maximum allowed size is: " + MaxContentLength + " MB";
                //ModelState.AddModelError("File", "Your file is too large, maximum allowed size is: " + MaxContentLength + " MB");
                return Json(new { error = true, result = "Your file is too large, maximum allowed size is: " + MaxContentLength + " MB" }, JsonRequestBehavior.AllowGet);
            }

            else if (!AllowedFileExtensions.Contains(cvFile.FileName.Substring(cvFile.FileName.LastIndexOf('.'))))
            {
                //ViewBag.ErrorMessage = "Please upload a file with doc, docx or pdf extension";
                //ModelState.AddModelError("File", "Please upload a file with doc, docx or pdf extension");
                return Json(new { error = true, result = "Please upload a file with doc, docx, jpg, png, gif or pdf extension" }, JsonRequestBehavior.AllowGet);

            }
            else
            {
                var path = Path.Combine(Server.MapPath("~/Content/Upload"), cvName);
                var directory = new DirectoryInfo(Server.MapPath("~/Content/Upload"));

                if (directory.Exists == false)
                {
                    directory.Create();
                }
                cvFile.SaveAs(path);
                var newDocument = new DocumentPath();
                newDocument.FilePath = path;
                newDocument.FileType = fileType;
                newDocument.ExtensionType = cvFile.FileName.Substring(cvFile.FileName.LastIndexOf('.'));
                newDocument.UserId = userId;

                db.DocumentPaths.Add(newDocument);
                db.SaveChanges();

                return Json(new { error = false, result = "Document Uploaded Successfully" }, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpPost]
        public ActionResult AddExperience(Experience exp)
        {
            var userId = User.Identity.GetUserId();
            if (ModelState.IsValid)
            {
                exp.UserId = userId;
                var newVacancy = db.Experiences.Add(exp);

                db.SaveChanges();
                return Json(new { Result = newVacancy.Id }, JsonRequestBehavior.AllowGet);
                
            }

            return Json(new { error = true, result = "Could not save experience" }, JsonRequestBehavior.AllowGet);
            
        }

        [HttpPost]
        public ActionResult AddEducation(EducationDetail edu)
        {
            var userId = User.Identity.GetUserId();
            if (ModelState.IsValid)
            {
                edu.UserId = userId;
                var newEdu = db.EducationDetails.Add(edu);

                db.SaveChanges();
                return Json(new { Result = newEdu.Id }, JsonRequestBehavior.AllowGet);

            }

            return Json(new { error = true, result = "Could not save education details" }, JsonRequestBehavior.AllowGet);
            
        }

        public ActionResult DeleteEducation(int id)
        {
            EducationDetail education = db.EducationDetails.Find(id);
            db.EducationDetails.Remove(education);
            db.SaveChanges();
            return Json(new { error = true, Result = "Education details deleted" }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult DeleteExperience(int id)
        {
            Experience exp = db.Experiences.Find(id);
            db.Experiences.Remove(exp);
            db.SaveChanges();
            return Json(new { error = true, Result = "Experience details deleted" }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult SuccessMessage()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}