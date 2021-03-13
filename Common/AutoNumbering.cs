using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace TechCenter
{
    public class AutoNumbering : IPlugin
    {
        IPluginExecutionContext context;
        IOrganizationService organizationService;

        public void Execute(IServiceProvider serviceProvider)
        {
            context = (IPluginExecutionContext)
                    serviceProvider.GetService(typeof(IPluginExecutionContext));

            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity)
            {
                Entity targetEntity = (Entity)context.InputParameters["Target"]; // New Tech Case

                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                    organizationService = serviceFactory.CreateOrganizationService(context.UserId);



                Entity autoNumberEnt = getAutoNumber(targetEntity.LogicalName);

                int counter = autoNumberEnt.GetAttributeValue<int>("pw_currentnumber");
                string count = autoNumberEnt.GetAttributeValue<string>("pw_prefix") + counter + autoNumberEnt.GetAttributeValue<string>("pw_suffix");
                counter++;

                targetEntity["pw_name"] = count;

                autoNumberEnt["pw_currentnumber"] = counter;
                organizationService.Update(autoNumberEnt);
            }

        }

        private Entity getAutoNumber(String entityName)
        {
            Entity resultEnt = null;

            QueryExpression queryExpression = new QueryExpression("pw_autonumbering"); // FROM "pw_autonumber"
            queryExpression.ColumnSet = new ColumnSet("pw_entityname", "pw_currentnumber", "pw_prefix", "pw_suffix"); // SELECT "pw_entityname", "pw_currentnumber", "pw_prefix", "pw_suffix"
            queryExpression.Criteria.AddCondition("pw_entityname", ConditionOperator.Equal, entityName); // WHERE "pw_entityname" == entityName

            EntityCollection entityCollection = organizationService.RetrieveMultiple(queryExpression);

            if(entityCollection.Entities.Count > 0)
            {
                resultEnt = entityCollection[0];
            }

            return resultEnt;
        }
    }
}
