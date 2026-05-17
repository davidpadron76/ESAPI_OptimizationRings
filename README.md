# 🧅 ESAPI Optimization Rings Generator

## 📖 Overview
The **ESAPI Optimization Rings Generator** is an open-source clinical tool developed for the Varian Eclipse Treatment Planning System (TPS). Creating manual dose containment shells for VMAT and IMRT planning can be tedious and highly prone to overlapping errors, which often confuse the optimization engine (Photon Optimizer). 

This script solves this by using advanced Boolean logic to automatically generate up to three mutually exclusive containment rings around target volumes, ensuring perfect geometric boundaries for dose gradients.

## ✨ Key Features
* **Multi-Target Merging:** Automatically detects and merges multiple selected PTVs into a single base structure before expansion.
* **The "Onion Effect" (Zero Overlap):** Intelligently prevents ring overlapping by automatically subtracting inner shells from outer ones.
* **Skin Sparing:** Automatically crops the generated rings inside the patient’s external body contour by a user-defined safety margin.
* **OAR Intersections (Smart Cropping):** If a generated ring intersects with a selected Organ at Risk (OAR), the script automatically extracts that overlapping segment into a new sub-structure (e.g., `zR1_in_Rectum`). This allows the dosimetrist to apply distinct, precise optimization weights to the specific overlapping volume.
* **Efficient UI Workflow:** Features a fast "zero-click" approach. Rings with a thickness set to `0.0` are automatically bypassed, allowing for rapid generation of 1, 2, or 3 rings without navigating extra drop-down menus.

## 💻 System Requirements
* **Eclipse TPS:** Version 15.5 or higher.
* **.NET Framework:** Compatible with your clinic's ESAPI version (e.g., 4.5 for v15.6, or 4.6+ for v16+).

## 🛠️ Installation & Compilation
To ensure proper functionality and UI rendering, this project should be compiled into a `.dll` library.

1. Clone or download this repository to your local machine.
2. Open the solution file (`.sln`) using **Visual Studio**.
3. Build the solution (`Ctrl + Shift + B` or `Build > Build Solution`).
4. Locate the compiled `.dll` file inside the `bin\Debug` or `bin\Release` folder.
5. In Eclipse, open the Script Runner, navigate to the folder containing your compiled `.dll`, and execute it.

## 🚀 How to Use
1. Open a Patient and a Structure Set in Eclipse.
2. Run the compiled Optimization Rings `.dll`.
3. In the UI window:
   * Select your Target(s) (e.g., PTVs).
   * Select the Organs at Risk (OARs) you want the rings to respect.
   * Define the distance from the PTV and the thickness for up to 3 rings (set thickness to `0` to skip a ring).
   * Define the Skin Sparing margin.
4. Click **Generate** and let the Boolean engine do the rest.

## 📄 License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## ⚠️ Clinical Disclaimer
**For Research and Educational Purposes Only.** This software is provided "as is", without warranty of any kind. It is the sole responsibility of the clinical user (Medical Physicist or Dosimetrist) to strictly verify and validate all generated contours and structures before using them for clinical patient treatment.
