# Quick Testing Checklist - Cura520 Auth System

## Pre-Testing Setup
- [ ] Delete database or run migrations
- [ ] Application starts and seeds data
- [ ] Admin account created: `CuraAdmin` / `Admin123*`
- [ ] No build errors

---

## REGISTRATION (Signup) Tests

### Happy Path
- [ ] Fill all registration fields correctly
- [ ] Click Register
- [ ] See "Registration successful!" message
- [ ] Redirected to Login page
- [ ] Confirmation email received
- [ ] User visible in AspNetUsers table
- [ ] Patient record visible in Patients table with all fields
- [ ] User has "Patient" role

### Validation Tests
- [ ] Empty First Name ? shows required error
- [ ] First Name with 1 character ? shows "minimum 2" error
- [ ] First Name with 30 characters ? shows "maximum 25" error
- [ ] Invalid email format ? shows "Invalid email" error
- [ ] Email already exists ? shows error (if implemented)
- [ ] Username < 3 characters ? shows error
- [ ] Password < 8 characters ? shows "at least 8" error
- [ ] Passwords don't match ? shows "do not match" error
- [ ] Invalid phone format ? shows "Invalid phone" error
- [ ] Missing required fields ? shows errors

### Email Confirmation
- [ ] Confirmation link in email works
- [ ] Link takes to ConfirmEmail action
- [ ] See "Email confirmed successfully!" message
- [ ] EmailConfirmed = 1 in database
- [ ] Link expires if clicked after token expiration

---

## LOGIN Tests

### Happy Path
- [ ] Login with confirmed email + correct password ? SUCCESS
- [ ] User redirected to Customer home
- [ ] User is authenticated
- [ ] Session/cookie created if "Remember me" checked

### Negative Tests
- [ ] Login with WRONG password ? "Invalid Login Attempt"
- [ ] Login with non-existent user ? "Invalid Login Attempt"
- [ ] Login with UN-CONFIRMED email ? "You need to confirm your email"
- [ ] Login with empty fields ? validation errors shown

### Validation Display
- [ ] Validation summary is visible
- [ ] Error messages are clear and helpful

---

## EMAIL CONFIRMATION - RESEND Tests

### Happy Path
- [ ] Click "Resend Email Confirmation" from login
- [ ] Enter email address of unconfirmed user
- [ ] New confirmation email sent
- [ ] Link in new email works
- [ ] Can now login

### Edge Cases
- [ ] Resend with already-confirmed email ? "Email already confirmed"
- [ ] Resend with non-existent email ? "No account found"

---

## PASSWORD RESET Tests

### Happy Path
1. **Initiate Reset**
   - [ ] Click "Forgot Password"
   - [ ] Enter email
   - [ ] OTP email sent
   
2. **Validate OTP**
   - [ ] OTP is 6 digits
   - [ ] OTP is random/unpredictable
   - [ ] Enter OTP on validation page
   - [ ] Redirected to Reset Password page

3. **Reset Password**
   - [ ] Enter new password (8+ chars)
   - [ ] Confirm password matches
   - [ ] See "Password reset successfully!"
   - [ ] Can login with new password

### OTP Validation Tests
- [ ] Invalid OTP (random 6 digits) ? "Invalid OTP"
- [ ] Expired OTP (after 10 mins) ? "OTP Expired"
- [ ] Reused OTP (second attempt) ? "Invalid OTP"
- [ ] OTP with letters entered ? only numbers accepted
- [ ] OTP field has placeholder "000000"

### Password Reset Tests
- [ ] Passwords don't match ? "Passwords do not match"
- [ ] Password < 8 characters ? "at least 8 characters"
- [ ] Valid password accepted ? reset successful

### Rate Limiting
- [ ] Can request OTP multiple times per session
- [ ] After 10 requests in 24 hours ? "exceeded maximum"

### Security Tests
- [ ] URL manipulation (changing userId in URL) ? error/redirect
- [ ] Direct access to ResetPassword without OTP validation ? redirect

---

## LOGOUT Tests

- [ ] Click Logout button
- [ ] User signed out
- [ ] Redirected to specified logout redirect
- [ ] Cannot access protected pages without logging back in

---

## AUTHENTICATION/AUTHORIZATION Tests

### Protected Routes (when NOT logged in)
- [ ] `/Customer/Home/Index` ? redirects to login
- [ ] `/Admin/Home/Index` ? redirects to login
- [ ] `/Doctor/...` ? redirects to login
- [ ] `/Receptionist/...` ? redirects to login

### Protected Routes (as Patient)
- [ ] `/Customer/Home/Index` ? ALLOWED ?
- [ ] `/Admin/Home/Index` ? Access Denied (403)
- [ ] `/Doctor/...` ? Access Denied (403)

### Public Routes
- [ ] `/Identity/Account/Login` ? always accessible
- [ ] `/Identity/Account/Register` ? always accessible
- [ ] `/Identity/Account/ForgetPassword` ? always accessible

---

## DATABASE VERIFICATION

### After Registration
```
AspNetUsers table:
- UserName = entered username
- Email = entered email
- FirstName = entered first name
- LastName = entered last name
- Address = entered address (or empty)
- Type = 0 (Patient)
- EmailConfirmed = 0 (initially) ? 1 (after confirmation)

Patients table:
- FirstName = matches AspNetUsers
- LastName = matches AspNetUsers
- DateOfBirth = entered DOB
- Gender = entered gender
- PhoneNumber = entered phone
- BloodType = entered blood type
- Allergies = entered allergies (or empty)
- ApplicationUserId = matches AspNetUsers.Id

AspNetUserRoles table:
- UserId = user's ID
- RoleId = Patient role ID
```

---

## EDGE CASES / SECURITY

- [ ] Can't register with same email twice
- [ ] Can't register with same username twice
- [ ] OTP generation is truly random (not sequential)
- [ ] Session/cookies are secure (HTTPS only, HTTPOnly flag)
- [ ] User IDs not visible in form fields
- [ ] Passwords never echoed in responses
- [ ] No email enumeration attacks (forgot password doesn't reveal accounts)

---

## PERFORMANCE

- [ ] Registration completes in < 2 seconds
- [ ] Login completes in < 1 second
- [ ] Email sending is asynchronous (doesn't block UI)
- [ ] OTP validation is instant

---

## ERROR HANDLING

- [ ] All errors display user-friendly messages
- [ ] No technical stack traces shown to users
- [ ] Errors logged server-side
- [ ] No sensitive data in error messages

---

## FINAL SIGN-OFF

**Tester Name:** ___________________  
**Date:** ___________________  
**All Tests Passed:** ? YES ? NO  
**Issues Found:** ___________________  

