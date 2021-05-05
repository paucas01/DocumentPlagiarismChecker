/*
    Copyright (C) 2018 Fernando Porrino Serrano.
    This software it's under the terms of the GNU Affero General Public License version 3.
    Please, refer to (https://github.com/FherStk/DocumentPlagiarismChecker/blob/master/LICENSE) for further licensing details.
 */
 
using System;
using System.Linq;
using System.Collections.Generic;
using DocumentPlagiarismChecker.Core;
using DocumentPlagiarismChecker.Scores;

namespace DocumentPlagiarismChecker.Comparators.DocumentWordCounter
{
    /// <summary>
    /// The Word Counter Comparator reads a pair of files and counts how many words and how many times appear on each file, and then calculates
    /// how many of those appearences matches between documents. So, two documents with the same amount of the same words can be a copy with
    /// a high level of provability.
    /// </summary>
    /// <typeparam name="Document"></typeparam>
    internal class Comparator: Core.BaseComparator<Document>
    {  
        /// <summary>
        /// Crea una nueva instancia del comparador.
        /// </summary>
        /// <param name="fileLeftPath">La ubicación del fichero del lado izquierdo.</param>
        /// <param name="fileRightPath">La ubicación del fichero del lado derecho.</param>
        /// <param name="settings">La instancia de ajustes que usará el comparador.</param>
        /// <returns></returns>
        public Comparator(string fileLeftPath, string fileRightPath, Settings settings): base(fileLeftPath, fileRightPath, settings){
        }  
        
        /// <summary>
        /// Cuenta cuántas palabras y cuantas veces aparecen en cada documento, y comprueba el porcentaje de coincidencia de las palabras.
        /// </summary>
        /// <returns>The matching's results.</returns>
        public override ComparatorMatchingScore Run(){
            //Contando la aparición de palabras por cada documento(izquierda y derecha).
            Dictionary<string, int[]> counter = new Dictionary<string, int[]>();
            foreach(string word in this.Left.WordAppearances.Select(x => x.Key)){
                if(!counter.ContainsKey(word)) counter.Add(word, new int[]{0, 0});
                counter[word][0] += Left.WordAppearances[word];
            }

            foreach(string word in this.Right.WordAppearances.Select(x => x.Key)){
                if(!counter.ContainsKey(word)) counter.Add(word, new int[]{0, 0});
                counter[word][1] += Right.WordAppearances[word];
            }

            //Contando apariciones de palabras de fichero de muestra, para ignorar las de ficheros anteriores.
            if(this.Sample != null){
                 foreach(string word in this.Sample.WordAppearances.Select(x => x.Key)){
                    if(counter.ContainsKey(word)){
                        counter[word][0] = Math.Max(0, counter[word][0] - Sample.WordAppearances[word]);
                        counter[word][1] = Math.Max(0, counter[word][1] - Sample.WordAppearances[word]);
                        
                        if(counter[word][0] == 0 && counter[word][1] == 0)
                            counter.Remove(word);
                    }                    
                }
            }

            //Definiendo los encabezados de los resultados
            ComparatorMatchingScore cr = new ComparatorMatchingScore(this.Left.Name, this.Right.Name, "Document Word Counter", DisplayLevel.FULL);            
            cr.DetailsCaption = new string[] { "Word", "Left count", "Right count", "Match" };
            cr.DetailsFormat = new string[]{"{0}", "{0}", "{0}", "{0:P2}"};

            //Calcular el matching para cada palabra individual.            
            foreach(string word in counter.Select(x => x.Key)){                
                int left = counter[word][0];
                int right = counter[word][1];                
                float match = (left == 0 || right == 0 ? 0 : (left < right ? (float)left / (float)right : (float)right / (float)left));

                cr.AddMatch(match);
                cr.DetailsData.Add(new object[]{word, left, right, match});                
            }                                    
            
            return cr;
        }        
    }   
}