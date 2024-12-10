using System.Net.Mail;
using System.Net;
using ExpenseTracker.Data;
using ExpenseTracker.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Controllers
{

    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }
        // Register GET
        public IActionResult Register()
        {
            return View();
        }

        // Register POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if username exists
                var userExists = await _context.Users
                    .AnyAsync(u => u.Username == model.Username || u.Email == model.Email);

                if (userExists)
                {
                    TempData["RegisterFail"] = "User with the same username or email already exists.Please Register Again!!";
                    ModelState.AddModelError("", "User with the same username or email already exists.");
                    return View(model);
                }

                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);
                var user = new User
                {
                    Username = model.Username,
                    Email = model.Email,
                    Password = hashedPassword
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return RedirectToAction("Login");
            }

            return View(model);
        }

      
        // Login GET
        public IActionResult Login()
        {
            return View();
        }

        // Login POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password, bool rememberMe)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                HttpContext.Session.SetString("Username", username);

                user.RememberMe = rememberMe;
                _context.Update(user);
                await _context.SaveChangesAsync();

                if (rememberMe)
                {
                    // Set persistent "Remember Me" cookie
                    var cookieOptions = new CookieOptions
                    {
                        Expires = DateTime.Now.AddDays(7), // Set cookie to expire in 7 days
                        IsEssential = true, // Mark the cookie as essential
                        HttpOnly = true // Make the cookie accessible only by the server
                    };
                    Response.Cookies.Append("Username", username, cookieOptions); // Store the username in the cookie
                }

                // Redirect to Dashboard after successful login
                return RedirectToAction("Index", "Dashboard");
            }

            ModelState.AddModelError("", "Invalid login attempt.");
            TempData["LoginFail"] = "Please Login again!!";
            return View();
        }

        //Get Profile
        public async Task<IActionResult> Profile()
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null)
            {
                // Redirect to the login page if no username is in the session
                return RedirectToAction("Login", "Account");
            }
            ViewData["Username"] = username;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                
                return RedirectToAction("Login", "Account");
            }
                ViewData["UserImagePath"] = user.ImagePath;
            return View(user);
        }


        // GET: Account/EditProfile
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var username = HttpContext.Session.GetString("Username");
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Login", "Account");
            }
            ViewData["Username"] = username;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                return NotFound();
            }
            ViewData["UserImagePath"] = user.ImagePath;
            return View(user);
        }


        //Post Edit Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(int id, [Bind("UserId,Email,Phone,Address,Note")] User model, IFormFile? Image)
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                var user = await _context.Users
                        .FirstOrDefaultAsync(u => u.Username == username);
                user.Email = model.Email;
                user.Phone = model.Phone;
                user.Address = model.Address;
                user.Note = model.Note;

                if (Image != null)
                {
                    // Delete the existing image if it exists
                    if (!string.IsNullOrEmpty(user.ImagePath))
                    {
                        var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ImagePath.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Save the new image
                    var fileName = Guid.NewGuid().ToString() + "_" + Image.FileName;
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await Image.CopyToAsync(stream);
                    }

                    user.ImagePath = "/images/" + fileName;
                }

                _context.Update(user);
                await _context.SaveChangesAsync();
                return RedirectToAction("Profile");
            }
           
            return RedirectToAction("EditProfile");

        }


        // GET: ChangePassword
        public IActionResult ChangePassword()
        {
            var username = HttpContext.Session.GetString("Username");
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user != null)
            {

                ViewData["Username"] = user.Username;
                ViewData["UserImagePath"] = user.ImagePath;
            }
            return View();
        }

        // POST: ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            // Ensure user is logged in using session
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                // Retrieve the user from the database using their username
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Verify the old password
                if (!BCrypt.Net.BCrypt.Verify(model.OldPassword, user.Password))
                {
                    ModelState.AddModelError(string.Empty, "The old password is incorrect.");
                    return View(model);
                }

                // Hash the new password and update the user's record
                user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                // Optional: Clear session and force the user to log in again
                HttpContext.Session.Clear();

                return RedirectToAction("Login", "Account");
            }

            // If ModelState is invalid, return the form with validation messages
            return View(model);
        }

        // GET: ForgotPassword
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(User model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (user == null)
                {
                    TempData["LoginFail"] = "User with this email does not exist.";
                    return RedirectToAction("ForgotPassword");
                }
                

                // Generate OTP
                var otp = GenerateOTP();

                // Send OTP via email
                var emailSent = await SendOTPEmail(user.Email, otp);
                if (emailSent)
                {
                    // Store OTP in session
                    HttpContext.Session.SetString("OTP", otp);
                    HttpContext.Session.SetString("Email", model.Email);

                    // Redirect to VerifyOTP page
                    return RedirectToAction("VerifyOTP");
                }

                TempData["LoginFail"] = "Error sending OTP. Please try again!";
                return RedirectToAction("ForgotPassword");
            }

            // If model is invalid, stay on the same page
            return View(model);
        }



        //generate OTP
        private string GenerateOTP()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        //Send OTP Email
        private async Task<bool> SendOTPEmail(string email, string otp)
        {
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("airiyuki123@gmail.com", "yukiairi"),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("airiyuki123@gmail.com"),
                Subject = "Your OTP Code",
                Body = $"Your OTP code is: {otp}",
                IsBodyHtml = false,
            };
            mailMessage.To.Add(email);

            try
            {
                await smtpClient.SendMailAsync(mailMessage);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        // GET: Verify OTP
        public IActionResult VerifyOTP()
        {
            return View();
        }

        // Logout
        public async Task<IActionResult> Logout()
        {
            // Log out the user by clearing their authentication cookie
            //await HttpContext.SignOutAsync();
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
    }
}
