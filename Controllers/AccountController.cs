using System.Net.Mail;
using System.Net;
using ExpenseTracker.Data;
using ExpenseTracker.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Macs;

namespace ExpenseTracker.Controllers
{

    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly EmailSender _emailSender;
        public AccountController(ApplicationDbContext context, EmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
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

                // Generate verification token
                var token = Guid.NewGuid().ToString();
                var verificationLink = Url.Action("VerifyEmail", "Account", new { token = token }, Request.Scheme);

                // Store the token in the user record or a temporary table for later verification
                user.EmailVerificationToken = token;
                await _context.SaveChangesAsync();

                // Send verification email
                await SendVerificationEmail(user.Email, verificationLink);

               return RedirectToAction("Login");
              // return RedirectToAction("CheckEmail");
            }

            return View(model);
        }


        private async Task SendVerificationEmail(string email, string verificationLink)
        {
            var subject = "Email Verification";
            var body = $"Please click the following link to verify your email: <a href='{verificationLink}'>Verify Email</a>";

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("airiyuki123@gmail.com", "yiib ypjt qdla udzo"), 
                EnableSsl = true
            };

            var message = new MailMessage("airiyuki123@gmail.com", email, subject, body)
            {
                IsBodyHtml = true 
            };

            await smtpClient.SendMailAsync(message);
        }

        public async Task<IActionResult> VerifyEmail(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "Invalid verification token.";
                return RedirectToAction("Login");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.EmailVerificationToken == token);

            if (user == null)
            {
                TempData["VerifyError"] = "Invalid verification link or token.";
                return RedirectToAction("Login");
            }

            // Mark the email as verified
            user.EmailVerified = true;
            user.EmailVerificationToken = null; // Clear the token once verified

            await _context.SaveChangesAsync();

            TempData["VerifySuccess"] = "Your email has been verified successfully. You can now log in.";

            // Return the view with success message
            // return View("VerifyEmail");
            return RedirectToAction("Login");
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
            // Find the user by username
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                TempData["LoginFail"] = "Invalid username or password.";
                return View();
            }

            // Check if the password is correct
            if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                TempData["LoginFail"] = "Invalid username or password.";
                return View();
            }

            // Check if the email is verified
            if (!user.EmailVerified)
            {
                TempData["LoginFail"] = "Your email is not verified. Please check your email for the verification link.";
                return View();
            }

            // Email verified and password is correct, proceed with login
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
        
        //Get Profile
        [HttpGet]
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
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if the email exists in the database
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "The email address is not registered.");
                return View(model);
            }

            // Generate a reset token
            var resetToken = Guid.NewGuid().ToString();
            user.PasswordResetToken = resetToken;
            user.PasswordResetTokenExpires = DateTime.UtcNow.AddHours(1); // Token valid for 1 hour
            await _context.SaveChangesAsync();

            // Create a reset link
            var resetLink = Url.Action("ResetPassword", "Account", new { token = resetToken }, Request.Scheme);

            // Send the reset link via email
            await SendResetPasswordEmail(model.Email, resetLink);

            model.EmailSent = true; // Indicate email was sent
            return View(model); // Optionally, redirect to a confirmation page
        }

        private async Task SendResetPasswordEmail(string email, string resetLink)
        {
            var subject = "Password Reset Request";
            var body = $"Click the following link to reset your password: <a href='{resetLink}'>Reset Password</a>";

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("airiyuki123@gmail.com", "yiib ypjt qdla udzo"),
                EnableSsl = true
            };

            var message = new MailMessage("airiyuki123@gmail.com", email, subject, body)
            {
                IsBodyHtml = true
            };
            await smtpClient.SendMailAsync(message);
        }

        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "Invalid or missing reset token.";
                return RedirectToAction("ForgotPassword");
            }

            var model = new ResetPasswordModel { Token = token };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == model.Token
                                                                   && u.PasswordResetTokenExpires > DateTime.UtcNow);
            if (user == null)
            {
                TempData["Error"] = "Invalid or expired reset token.";
                return RedirectToAction("ForgotPassword");
            }

            // Update password
            user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            user.PasswordResetToken = null; // Clear the token
            user.PasswordResetTokenExpires = null; // Clear the expiration
            await _context.SaveChangesAsync();

            TempData["Success"] = "Your password has been reset successfully.";
            return RedirectToAction("Login");
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
