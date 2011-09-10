﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using System.Dynamic;

namespace Oak.Tests
{
    class describe_Prototype : nspec
    {
        dynamic blog;

        dynamic mix;

        dynamic inheritedMix;

        void before_each()
        {
            blog = new ExpandoObject();
            blog.Title = "Some Name";
            blog.body = "Some Body";
            blog.BodySummary = "Body Summary";
            mix = new Prototype(blog);
        }

        void describe_responds_to()
        {
            it["responds to property with exact casing"] = () => (mix as Prototype).RespondsTo("Title").should_be_true();

            it["it responds to property with case insensitive"] = () => (mix as Prototype).RespondsTo("title").should_be_true();

            it["it doesn't respond to property"] = () => (mix as Prototype).RespondsTo("foobar").should_be_false();
        }

        void describe_get_value_for_property()
        {
            it["retrieves value with exact casing"] = () => ((mix as Prototype).GetValueFor("Title") as string).should_be("Some Name");

            it["retrieves value with exact case insensitive"] = () => ((mix as Prototype).GetValueFor("title") as string).should_be("Some Name");

            it["throws invalid op if property doesn't exist"] = expect<InvalidOperationException>("This mix does not respond to the property FooBar.", () => (mix as Prototype).GetValueFor("FooBar"));
        }

        void when_retrieving_property_from_mix()
        {
            it["calls values for mixed entity"] = () =>
                (mix.Title as string).should_be("Some Name");

            it["calls value for mixed entity even if property's first letter doesn't match case"] = () =>
                (mix.title as string).should_be("Some Name");

            it["calls value for mixed entity even if property's first letter is capilized, but underlying property is lowercase"] = () =>
                (mix.Body as string).should_be("Some Body");

            it["ignores case for mixed entity"] = () =>
                (mix.bodysummary as string).should_be("Body Summary");
        }

        void when_setting_properyt_of_mix()
        {
            it["sets property on underlying expando"] = () =>
            {
                mix.Title = "Some other name";
                (blog.Title as string).should_be("Some other name");
            };

            it["sets property of underlying expando even if property's first letter doesn't match case"] = () =>
            {
                mix.title = "Some other name";
                (blog.Title as string).should_be("Some other name");
            };

            it["sets property of underlying expando even if property's first letter is capitalized, but underlying property is lowercase"] = () =>
            {
                mix.Body = "Some other name";
                (blog.body as string).should_be("Some other name");
            };

            it["ignores case for mixed entity"] = () =>
            {
                mix.bodysummary = "Blog Summary New";
                (blog.BodySummary as string).should_be("Blog Summary New");
            };
        }

        void inherited_mixed_with_defined_methods()
        {
            act = () => inheritedMix = new InheritedMix(blog);

            it["calls underlying property"] = () =>
                (inheritedMix.Title as string).should_be("Some Name");

            it["sets underlying property"] = () =>
            {
                inheritedMix.Title = "Some other name";
                (blog.Title as string).should_be("Some other name");
            };

            it["calls defined method"] = () =>
                (inheritedMix.FirstLetter() as string).should_be("S");

            it["calls defined property"] = () =>
                (inheritedMix.FirstName as string).should_be("Some");
        }

        void double_inheritance()
        {
            act = () => inheritedMix = new InheritedInheritedMix(blog);

            it["calls methods on root mix with"] = () =>
                (inheritedMix.Title as string).should_be("Some Name");

            it["calls method on first mix"] = () =>
                (inheritedMix.FirstLetter() as string).should_be("S");

            it["calls method on top most mix"] = () =>
                (inheritedMix.LastLetter() as string).should_be("e");
        }

        void given_a_blog()
        {
            before = () =>
            {
                blog = new ExpandoObject();
                blog.Title = "Working With Oak";
                blog.Body = "Oak is tight, yo.";
            };

            context["given that the dynamic blogged is wrapped with a mix"] = () =>
            {
                before = () =>
                    mix = new BlogEntry(blog);

                it["base properties are still accessible"] = () =>
                    (mix.Title as string).should_be("Working With Oak");

                it["base properties are still settable"] = () =>
                {
                    mix.Title = "Another Title";
                    (blog.Title as string).should_be("Another Title");
                };

                it["new properites provided by BlogEntry mix are available"] = () =>
                    ((bool)mix.IsValid()).should_be_true();

                it["properites defined in the mix do override base properties"] = () =>
                    (mix.Body as string).should_be("");
            };

        }

        void working_with_parameterless_mix_that_defines_properites_in_the_constructor()
        {
            before = () => mix = new ParameterlessMix();

            it["properties are accessible"] = () => (mix.FirstName as string).should_be("");

            context["tacking on properties after the fact is allowed"] = () =>
            {
                act = () => mix.Expando.NewProp = "new prop";

                it["new prop is accessible"] = () => (mix.NewProp as string).should_be("new prop");
            };

            context["tacking on methods after the fact is allowed"] = () =>
            {
                act = () => mix.Expando.NewProp = new Func<string, string>((s) => s.ToUpper());

                it["new method is accessible"] = () => (mix.NewProp("hello") as string).should_be("HELLO");
            };
        }
    }

    public class BlogEntry : Prototype
    {
        public BlogEntry(object o)
            : base(o)
        {

        }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Expando.Title);
        }

        public string Body
        {
            get
            {
                return "";
            }
        }
    }

    public class InheritedInheritedMix : InheritedMix
    {
        public InheritedInheritedMix(object o)
            : base(o)
    	{
    				
    	}

        public string LastLetter()
        {
            return (Expando.Title as string).Last().ToString();
        }
    }

    public class InheritedMix : Prototype
    {
        public InheritedMix(object o)
            : base(o)
        {

        }

        public string FirstLetter()
        {
            return (Expando.Title as string).First().ToString();
        }

        public string FirstName
        {
            get
            {
                return (Expando.Title as string).Split(' ').First();
            }
        }
    }

    public class ParameterlessMix : Prototype
    {
        public ParameterlessMix()
        {
            Expando.FirstName = "";
            Expando.LastName = "";
        }
    }
}