using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Text;

namespace AzureFunctionsBulkAdd.Helpers
{
    class DynamicMapper
    {

        /// <summary>
        ///  Converts an object into a dynamic object
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static dynamic ToDynamic(object value)
        {
            IDictionary<string, object> expando = new ExpandoObject(); //instiate list

            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(value.GetType()))  //iterate over properties 
                expando.Add(property.Name, property.GetValue(value));    // add property name and value to expando object

            return expando as ExpandoObject;  //return expando as dyhnamic
        }


        /// <summary>
        /// 
        /// Take a list of objects and converts it to a list of dynamic objects for speed
        /// and extensibility
        /// 
        /// </summary>
        /// <param name="objs"></param>
        /// <returns></returns>
        public static List<dynamic> ToDynamics(List<object> objs)
        {
            //instantiate list
            List<dynamic> dynamicObjs = new List<dynamic>();


            //iterate over input objs and convert to dynamic
            foreach (var obj in objs)
            {

                dynamicObjs.Add(ToDynamic(obj)); // add to list


            }
            return dynamicObjs; //return list


        }

    }
}
