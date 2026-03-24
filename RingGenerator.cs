using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Windows.Controls;
using System.Collections.Generic;


namespace VMS.TPS
{
    public class Script
    {
        public void Execute(ScriptContext context)
        {
            // 1. Validaciones iniciales
            StructureSet ss = context.StructureSet;
            if (ss == null)
            {
                MessageBox.Show("Por favor, abre un plan o un Structure Set antes de ejecutar el script.", "Error Rings", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 2. Clasificar estructuras para las listas
            var possiblePTVs = ss.Structures.Where(s => s.Id.ToUpper().Contains("PTV") || s.DicomType == "PTV").ToList();
            var possibleOARs = ss.Structures.Where(s => !s.IsEmpty && s.DicomType != "EXTERNAL" && !possiblePTVs.Contains(s)).OrderBy(s => s.Id).ToList();

            if (!possiblePTVs.Any())
            {
                MessageBox.Show("No se encontró ninguna estructura tipo PTV en el Structure Set.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 3. Construir la Interfaz Gráfica (WPF)
            Window mainWindow = new Window
            {
                Title = "Optimization Rings Generator",
                Width = 450,
                Height = 650,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize
            };

            StackPanel mainPanel = new StackPanel { Margin = new Thickness(15) };

            // -- Sección A: Selección de Target --
            mainPanel.Children.Add(new TextBlock { Text = "1. Target Selection (Merge into zPTV_Total):", FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 5) });
            ListBox lstPTVs = new ListBox
            {
                SelectionMode = SelectionMode.Multiple,
                DisplayMemberPath = "Id",
                ItemsSource = possiblePTVs,
                Height = 80,
                Margin = new Thickness(0, 0, 0, 15)
            };
            mainPanel.Children.Add(lstPTVs);

            // -- Sección B: Configuración de Anillos --
            mainPanel.Children.Add(new TextBlock { Text = "2. Rings Configuration (Leave Thickness 0 to skip):", FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 5) });

            var pnlRings = new System.Windows.Controls.Primitives.UniformGrid { Columns = 3 };

            pnlRings.Children.Add(new TextBlock { Text = "Ring #", FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center });
            pnlRings.Children.Add(new TextBlock { Text = "Start Dist (cm)", FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center });
            pnlRings.Children.Add(new TextBlock { Text = "Thickness (cm)", FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center });

            // Anillo 1
            pnlRings.Children.Add(new TextBlock { Text = "zRing1:", VerticalAlignment = VerticalAlignment.Center });
            TextBox txtR1Start = new TextBox { Text = "0.0", Margin = new Thickness(5) };
            TextBox txtR1Thick = new TextBox { Text = "1.0", Margin = new Thickness(5) };
            pnlRings.Children.Add(txtR1Start); pnlRings.Children.Add(txtR1Thick);

            // Anillo 2
            pnlRings.Children.Add(new TextBlock { Text = "zRing2:", VerticalAlignment = VerticalAlignment.Center });
            TextBox txtR2Start = new TextBox { Text = "1.0", Margin = new Thickness(5) };
            TextBox txtR2Thick = new TextBox { Text = "2.0", Margin = new Thickness(5) };
            pnlRings.Children.Add(txtR2Start); pnlRings.Children.Add(txtR2Thick);

            // Anillo 3
            pnlRings.Children.Add(new TextBlock { Text = "zRing3:", VerticalAlignment = VerticalAlignment.Center });
            TextBox txtR3Start = new TextBox { Text = "3.0", Margin = new Thickness(5) };
            TextBox txtR3Thick = new TextBox { Text = "0.0", Margin = new Thickness(5) };
            pnlRings.Children.Add(txtR3Start); pnlRings.Children.Add(txtR3Thick);

            mainPanel.Children.Add(pnlRings);

            // -- Sección C: Skin Flash --
            StackPanel pnlSkin = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 15, 0, 15) };
            pnlSkin.Children.Add(new TextBlock { Text = "3. Skin Retraction Margin (cm): ", FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center });
            TextBox txtSkinFlash = new TextBox { Text = "0.5", Width = 50 };
            pnlSkin.Children.Add(txtSkinFlash);
            mainPanel.Children.Add(pnlSkin);

            // -- Sección D: OARs para Intersección --
            mainPanel.Children.Add(new TextBlock { Text = "4. Intersect OARs (Creates zR1_in_OAR etc.):", FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 5) });
            ListBox lstOARs = new ListBox
            {
                SelectionMode = SelectionMode.Multiple,
                DisplayMemberPath = "Id",
                ItemsSource = possibleOARs,
                Height = 100,
                Margin = new Thickness(0, 0, 0, 15)
            };
            mainPanel.Children.Add(lstOARs);

            // -- Sección E: Botón Ejecutar --
            Button btnGenerate = new Button
            {
                Content = "Generate Optimization Rings",
                Height = 40,
                FontWeight = FontWeights.Bold,
                Background = System.Windows.Media.Brushes.LightGreen
            };

            btnGenerate.Click += (sender, e) =>
            {
                List<Structure> selectedPTVs = lstPTVs.SelectedItems.Cast<Structure>().ToList();
                List<Structure> selectedOARs = lstOARs.SelectedItems.Cast<Structure>().ToList();

                if (!selectedPTVs.Any())
                {
                    MessageBox.Show("Debes seleccionar al menos un PTV.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                double[] startDists = new double[] { double.Parse(txtR1Start.Text), double.Parse(txtR2Start.Text), double.Parse(txtR3Start.Text) };
                double[] thicknesses = new double[] { double.Parse(txtR1Thick.Text), double.Parse(txtR2Thick.Text), double.Parse(txtR3Thick.Text) };
                double skinFlash = double.Parse(txtSkinFlash.Text);

                mainWindow.DialogResult = true;
                mainWindow.Close();

                // Llamada al motor
                GenerateOptimizationRings(context, selectedPTVs, startDists, thicknesses, skinFlash, selectedOARs);
            };

            mainPanel.Children.Add(btnGenerate);
            mainWindow.Content = mainPanel;
            mainWindow.ShowDialog();
        }

        // =========================================================================
        // MOTOR BOOLEANO Y DE MÁRGENES (Fase 2)
        // =========================================================================
        private void GenerateOptimizationRings(ScriptContext context, List<Structure> ptvs, double[] startsCm, double[] thicksCm, double skinMarginCm, List<Structure> oars)
        {
            try
            {
                context.Patient.BeginModifications();
                StructureSet ss = context.StructureSet;

                // 1. Fusionar PTVs en "zPTV_Total"
                RemoveStructureIfExists(ss, "zPTV_Total");
                Structure zPtvTotal = ss.AddStructure("PTV", "zPTV_Total");

                zPtvTotal.SegmentVolume = ptvs.First().SegmentVolume;
                foreach (var ptv in ptvs.Skip(1))
                {
                    zPtvTotal.SegmentVolume = zPtvTotal.SegmentVolume.Or(ptv.SegmentVolume);
                }

                // 2. Preparar el límite de la piel (Skin Retraction)
                Structure body = ss.Structures.FirstOrDefault(s => s.DicomType == "EXTERNAL");
                SegmentVolume safeBodyArea = null;
                if (body != null && !body.IsEmpty)
                {
                    safeBodyArea = body.SegmentVolume.Margin(-skinMarginCm * 10.0);
                }

                // 3. Generar Anillos y Evitar Solapamientos
                List<Structure> createdRings = new List<Structure>();
                int ringsGenerated = 0;
                int intersectionsGenerated = 0;

                for (int i = 0; i < 3; i++)
                {
                    if (thicksCm[i] <= 0.0) continue; // Saltar anillo si el grosor es 0

                    double innerMarginMm = startsCm[i] * 10.0;
                    double outerMarginMm = (startsCm[i] + thicksCm[i]) * 10.0;
                    string ringName = $"zRing{i + 1}";

                    RemoveStructureIfExists(ss, ringName);
                    Structure currentRing = ss.AddStructure("CONTROL", ringName);

                    // A: Crear anillo básico (Borde Exterior menos Borde Interior)
                    var outerVol = zPtvTotal.SegmentVolume.Margin(outerMarginMm);
                    var innerVol = zPtvTotal.SegmentVolume.Margin(innerMarginMm);
                    var ringVol = outerVol.Sub(innerVol);

                    // B: Restar anillos anteriores (Efecto Cebolla)
                    foreach (var prevRing in createdRings)
                    {
                        ringVol = ringVol.Sub(prevRing.SegmentVolume);
                    }

                    // C: Recortar por dentro de la piel
                    if (safeBodyArea != null)
                    {
                        ringVol = ringVol.And(safeBodyArea);
                    }

                    currentRing.SegmentVolume = ringVol;

                    // Si el anillo quedó vacío, lo borramos y seguimos
                    if (currentRing.IsEmpty)
                    {
                        ss.RemoveStructure(currentRing);
                        continue;
                    }

                    createdRings.Add(currentRing);
                    ringsGenerated++;

                    // 4. Intersección con OARs seleccionados
                    foreach (var oar in oars)
                    {
                        var intersection = currentRing.SegmentVolume.And(oar.SegmentVolume);

                        // Evaluar vacuidad de SegmentVolume usando una estructura temporal
                        string tmpName = $"__tmp_{Guid.NewGuid():N}";
                        Structure tmpStruct = ss.AddStructure("CONTROL", tmpName);
                        try
                        {
                            tmpStruct.SegmentVolume = intersection;
                            if (!tmpStruct.IsEmpty)
                            {
                                // Truncar nombre a 9 caracteres para cumplir límite de 16: "zR1_in_OARname9"
                                string oarName = oar.Id.Length > 9 ? oar.Id.Substring(0, 9) : oar.Id;
                                string intersectName = $"zR{i + 1}_in_{oarName}";

                                RemoveStructureIfExists(ss, intersectName);
                                Structure intersectStruct = ss.AddStructure("CONTROL", intersectName);
                                intersectStruct.SegmentVolume = intersection;

                                // D: Restar la intersección del anillo principal
                                currentRing.SegmentVolume = currentRing.SegmentVolume.Sub(intersection);
                                intersectionsGenerated++;
                            }
                        }
                        finally
                        {
                            // Asegurarse de eliminar la estructura temporal
                            if (tmpStruct != null)
                            {
                                ss.RemoveStructure(tmpStruct);
                            }
                        }
                    }
                }

                MessageBox.Show($"¡Optimización de geometría completada!\n\n" +
                                $"- PTVs fusionados en 'zPTV_Total'\n" +
                                $"- Anillos base generados: {ringsGenerated}\n" +
                                $"- Intersecciones OAR generadas: {intersectionsGenerated}",
                                "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error inesperado:\n{ex.Message}", "Error Crítico", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // =========================================================================
        // MÉTODOS AUXILIARES
        // =========================================================================
        private void RemoveStructureIfExists(StructureSet ss, string id)
        {
            var target = ss.Structures.FirstOrDefault(s => s.Id == id);
            if (target != null)
            {
                ss.RemoveStructure(target);
            }
        }
    }
}