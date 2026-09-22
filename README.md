# MCC Construction Website

A modern, secure, responsive construction company website developed for **MCC Construction**, providing company information, services, completed projects, customer enquiries, quotation requests, and a secure administration dashboard.

The application is built using **ASP.NET Core MVC**, **Entity Framework Core**, and **SQL Server LocalDB**, with a focus on responsive design, secure administration, data persistence, and practical business functionality.

---

## 📌 Project Overview

The MCC Construction Website was developed to provide the company with a professional online presence while allowing customers to:

- Learn about MCC Construction
- View company services
- Explore completed projects
- Browse the project gallery
- Submit contact enquiries
- Request construction quotations
- Upload supporting documents or images with quote requests

The system also provides a secure administrator area where authorized administrators can:

- View all customer enquiries
- View all quotation requests
- Read complete customer messages
- Review project descriptions
- Access submitted attachments
- Update enquiry statuses
- Update quotation statuses
- Monitor new submissions
- Securely log out of the administration area

---

## 🏗️ About MCC Construction

**MCC Construction** is a South African construction company established in 1964.

The website presents information about the company's construction services, projects, certifications, and contact information.

### Core Services

- Developments
- Commercial Construction
- Residential Construction
- Alterations
- Civil Construction

### Company Information

**MCC Construction**

Durban Head Office  
56 Newport Avenue  
Glen Ashley  
Durban North  
4051  
KwaZulu-Natal  
South Africa

---

# ✨ Features

## Public Website

### Home Page

The home page provides:

- Company introduction
- Construction-focused branding
- Key company information
- Services overview
- Project highlights
- Calls to action
- Contact information

### About Page

Provides information about:

- MCC Construction
- Company history
- Company background
- Certifications
- Experience
- Business information

### Services Page

Displays the construction services offered by MCC Construction.

### Projects Page

Provides information about selected completed construction projects.

### Gallery

Provides a visual gallery of MCC Construction projects and work.

### Contact Page

Customers can submit enquiries containing:

- Name
- Company
- Email
- Phone
- Subject
- Message

Submitted contact messages are stored in the database and become available to administrators.

### Quote Request

Customers can request a construction quotation by submitting:

- Company name
- Contact person
- Email
- Phone
- Project address
- Required service
- Budget
- Expected start date
- Project description
- Supporting attachment

Supported attachment formats include:

- PDF
- DOC
- DOCX
- JPG
- JPEG
- PNG

Attachments are restricted to a maximum size of **10 MB**.

---

# 🔐 Administration System

The application includes a protected administrator area.

## Administrator Login

Administrators authenticate through:

```text
/Admin/Login
