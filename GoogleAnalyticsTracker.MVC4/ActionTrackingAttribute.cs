using System;
using System.Web.Mvc;

namespace GoogleAnalyticsTracker.MVC4
{
    public class ActionTrackingAttribute
        : ActionFilterAttribute
    {
        private Func<ActionDescriptor, bool> _isTrackableAction;

        public Tracker Tracker { get; set; }

        public Func<ActionDescriptor, bool> IsTrackableAction
        {
            get
            {
                if (_isTrackableAction != null)
                {
                    return _isTrackableAction;
                }
                return action => true;
            }
            set { _isTrackableAction = value; }
        }

        public string ActionDescription { get; set; }
        public string ActionUrl { get; set; }

        public ActionTrackingAttribute()
            : this(null, null, null, null)
        {
        }

        public ActionTrackingAttribute(string trackingAccount, string trackingDomain)
            : this(trackingAccount, trackingDomain, null, null)
        {
        }

        public ActionTrackingAttribute(string trackingAccount)
            : this(trackingAccount, null, null, null)
        {
        }

        public ActionTrackingAttribute(string trackingAccount, string trackingDomain, string actionDescription, string actionUrl)
        {
            if (string.IsNullOrEmpty(trackingDomain) && System.Web.HttpContext.Current != null)
            {
                trackingDomain = System.Web.HttpContext.Current.Request.Url.Host;
            }

            Tracker = new Tracker(trackingAccount, trackingDomain, new CookieBasedAnalyticsSession(), new AspNetMvc4TrackerEnvironment());
            ActionDescription = actionDescription;
            ActionUrl = actionUrl;
        }

        public ActionTrackingAttribute(Tracker tracker)
            : this(tracker, action => true)
        {
        }

        public ActionTrackingAttribute(Tracker tracker, Func<ActionDescriptor, bool> isTrackableAction)
        {
            Tracker = tracker;
            IsTrackableAction = isTrackableAction;
        }

        public ActionTrackingAttribute(string trackingAccount, string trackingDomain, Func<ActionDescriptor, bool> isTrackableAction)
        {
            Tracker = new Tracker(trackingAccount, trackingDomain, new CookieBasedAnalyticsSession(), new AspNetMvc4TrackerEnvironment());
            IsTrackableAction = isTrackableAction;
        }

        public static void RegisterGlobalFilter(string trackingAccount, string trackingDomain)
        {
            GlobalFilters.Filters.Add(new ActionTrackingAttribute(trackingAccount, trackingDomain));
        }

        public static void RegisterGlobalFilter(Tracker tracker)
        {
            GlobalFilters.Filters.Add(new ActionTrackingAttribute(tracker));
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (IsTrackableAction(filterContext.ActionDescriptor))
            {
                OnTrackingAction(filterContext);
            }
        }

        public virtual string BuildCurrentActionName(ActionExecutingContext filterContext)
        {
            return ActionDescription ??
                   filterContext.ActionDescriptor.ControllerDescriptor.ControllerName + " - " +
                   filterContext.ActionDescriptor.ActionName;
        }

        public virtual string BuildCurrentActionUrl(ActionExecutingContext filterContext)
        {
            var request = filterContext.RequestContext.HttpContext.Request;

            return ActionUrl ?? (request.Url != null ? request.Url.PathAndQuery : "");
        }

        public virtual void OnTrackingAction(ActionExecutingContext filterContext)
        {
            // todo: we should await the result
            Tracker.TrackPageViewAsync(
                filterContext.RequestContext.HttpContext,
                BuildCurrentActionName(filterContext),
                BuildCurrentActionUrl(filterContext));
        }
    }
}