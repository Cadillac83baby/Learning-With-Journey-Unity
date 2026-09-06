#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace LearningWithJourney.EditorTools
{
    public static class LWJAlphabetMatchCardBackRepairV5
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/AlphabetMatchWorld.unity";
        const string GeneratedFolder = "Assets/LearningWithJourney/Generated/AlphabetMatch";
        const string CardBackPath = GeneratedFolder + "/LWJ_Match_Card_Back_V5.jpg";
        const string LogoObjectName = "LearningWithJourneyCardBackLogo";

        // The approved Learning with Journey card-back art is embedded here so this repair
        // does not depend on Git/GitHub transporting a binary image correctly.
        const string CardBackBase64 =
            "/9j/4AAQSkZJRgABAQAAAQABAAD/2wBDAAUDBAQEAwUEBAQFBQUGBwwIBwcHBw8LCwkMEQ8SEhEPERETFhwXExQaFRERGCEYGh0dHx8f" +
            "ExciJCIeJBweHx7/2wBDAQUFBQcGBw4ICA4eFBEUHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4e" +
            "Hh7/wAARCQEnAMADASIAAhEBAxEB/8QAHAAAAgIDAQEAAAAAAAAAAAAABgcFBAMIAQIJ/8QAPxAAAgEDAwIEAwYEBQUBAAAAAQIDBAURAAYS" +
            "ITFBEyJRYXGBBxQykQgjQqGx0RYzUqLB8PEVFyRScv/EABkBAAMBAQEAAAAAAAAAAAAAAAABAgMEBf/EACIRAQEAAgIBBAMBAAAAAAAAAAAB" +
            "AhEDIRIxQVEiMmFxgf/aAAwDAQACEQMRAD8A5fUPrpuBLChSJUoRmJ79wqfzAZqKjSHc1/J8Kdkt8vpm+oYo9p7q4OQmCk8z0YwPZ1D/AK" +
            "2PbYp0p5bdDoKrsS3JgSeB69hkk9WWDliZTkyBhJX+3Xr7NaZju7dCf2H9xq8qVNpSpIygHXPIwD5dDY+fXqO9bavsqVcjP8AS9u77vFU" +
            "X1XLETe2tc8RLbbf9kH8v6qK70xxRX12H39dU19FVzQ7VPu99pX4qZxtZrDHf1rjVboWw/SH9RxTnaPiAbtfofKxqZ1hPtae0J7WnAOM" +
            "MQklnqQSMZ+PrT0lRLkgC2IYyrlIygfMd8n08Pu1a16oW1dtJr8J9F44pTDhYE/n1e3T5CspVt7kVblQRt2Ey4A6jlmrVa5oTpC9jdMd" +
            "pz5ycfCmxnqf0OPnUbZU9xaWCVADnWHSpSAByB0YztHn98DwvCW8FR6jRLglwWwTEdlBJPcZ4xn0qrVpSpvMD2UhPOEe9Vnmru2dnY4Yl" +
            "M+W3U9pT72zwqqyJ62MJrbopQCQlQD5biO2N8D2ONzW0bVPS1YZFtNvTXdiN3EH0CrKqrM5wUKT7/nhT56tZQi5bSQfDByc4Yd7X+gw" +
            "O3X7Vct66MvW10mGNZuNQE9wmxOFHvb0Uk+tZtTHeyyj8sCqgUIB0+J7uPqO9Wl4L5aL4bbiDoJyHHCM/04qWwT24BPApJzjJAA+Xbz" +
            "rOqdzUcztiZR1JMbn3jFc0pHxIH37+fWss2y6xPIWBhg4wevzPprfUutqoRBbWuTPgyTn/WGNx5GtrxdRRyeWpcne/yKx4pScknnIwCQ" +
            "fQe3b2qhJGvGo+VutDfWgBtuRuBPbGTntW1OGMJue+MZXYTKSCOvrgD5c6fnW43t9YV2zduSxuU4P7+9RAlvOVXGPXjU4O0ueM64j1" +
            "FtE0ejgjJiXV9pWikqCSQOxIGF9Ac4HH5d67fMbXKsxre1zVbWcS5O/G1R5hfhnY4z/Wxqp2dQx1ljIoI2Nfd26Do2OF+Bq8S5kE8d" +
            "2/rQ/41Vu3RTzHnwbV5Cn7wH95I48uMfD3x3qadCoESR5gz5QA/wDDDeQMjt6jrxq0nT9sxqrfPpWlEcRu/m8dyDx1j46fiKz2lYZX" +
            "mN1AtOBAKjoB5h+Qa91zrVFwtdHsY1IuM+Ma4OOBkMZY8vB+kqVh5FCLjgPx9fyqMZHvb7HUHQe2d+FQeo9au56nU7Z3QXMk3kQZ+6" +
            "Dz3PfPfNQSihmWQkeZPvLfD49dqmd8QzjipWJ5GUORg8d+tQk1pWMRkh5AjFz0fN6+2PqfKmjO1raEZMVzaFLA7v7tRDSm5jK8hAA" +
            "HnX2bA7HBDY71kxW9mCtZMQr74VGuIoMEYyO2+R7E7++9M6m3h8ukrE5wMsO+x3qac7rGxl2lDtvaWjbI6hhmPL8zj1JrIveYDo6" +
            "9ce3nXVzDccQ5CmORnP94qQ0kXjZWbYuHHGdrn0j4g+RrBGrviqkCXabKTnCYznGPD39tcdLh7rPZW0kGW2zjk24HHxuM4GIUPu0Hn" +
            "7dNflxjY6fR0pq3dK8z7yQfP5VFejlM7bHnp86ebETj7xA+/SrXjHq6J2TI4U8L4hV5E95jtwjLs30GKkRBJUKD+7ZAFPsBjP9Ote" +
            "ZOKUlXDOoCKyh4bUMTuzYA9cD6VN0npslW0T5yFAwHAGMgjPPf9KweoPdSoRSuo3+cckPI+Pgff3qK5jS4ddF0Za4Hcz7GicA7csxo" +
            "d7bWIjqe2PzqC4Ti00R8lJH3fM5z+VQVlxrJ8rKQp3jI3cZPbIHvrHV9b6cYirW3VJtXzj5XODhVChMAZJHT0yK4pRSb7pVDI5dw" +
            "Bzk5PXjV2KRhqdTMlIMhZ5Ck5+3P7ViylRuZH3xKuOB1+3oR6VHWfQQ0tJdQW5TsCB7oPGfbn3qtQ1fPY2V2krsUzxR6zFk9z/gKV" +
            "j6cR2NyvD5G1yHAUMc4bOe89Kc3EL2hs4khR22sPlh6+vBrIL2tCky5XxkkDGCc5zz7VZ8PSUe2zC0hlDKzfIPr2GB1rFqJIBxjlV" +
            "wTjzj0o1Kk2zM7W+56Q3cVCjPdyhgHd4/T3qS7mE0Fm80WMMSuI2nI5wcf3Vg1hV5QyHXKkgnrtPQ5rTuvsPa67a2e5lk7RKGwMx" +
            "I5x3qMTBqR5KQVI1JwFJJOeoGTU6O8V3N5b5w7P/ABjk4/nmqpHslWGJo/IpTOXBw22vzI2/vWrMOdpm7kNhmdrfXN0t1eRDZMqkq" +
            "M4PPbux6e9JsbbxytHdg0qSHFDDJz/PG7jGe9Oa8pzDl2CsXMuR6VQq+KGCwuR9pDjsMfdB98fhW1qWdq1k9HVCbT2txdxJFFcaJx" +
            "H5k+aSoGTgHOcehH1qn4ppQqJmyyuRYwCSOT3xVQW3jkuJLiSe4kdCIyrSrhgFJTg8jtGf1zV7wNvxk+zR75Ub81DYGUOwJGTjj8+9" +
            "a33QLVUklv5f20EghdrYrLNvd/uGFzkcY5Hc1nvpCSQcguARnqd3Gc8dKNR5YLM26O+72f8KQ3AB4z1P+VaReH9Tt5kWOPvSM8NyF" +
            "DLx6nHrU0uLWLcUBV3rJmVAcEc9M/n1rFk7rByuSd0v+sB6V3Vnw32V9Lc6pJZsVH81sYYyk55BPrr8qxDKsLckh3BzjHzwD61FSQ" +
            "B0J6A88dKryTlkE0T0W6WY4KPWXrz61p5ik4y3X0LqLht1aQeTlY5M8yFuSoJz7E8frVKV4ZOu8pY7Y6qfw4OOoI9etWkY2hggaR" +
            "IowWwQQcgcD1x+tdwo39nl4twT4+nHr0qzWpSnyjMtmnR7aS1uN3dWGdwZLKM45GQ2TjP5U7w9qU2V0+iwW8X2byQBlUHBHjOecc9" +
            "8VXw9qujWch0yzlvLiylCEkdc+nn9a4i1uJ4IliET94WX6fdqj1p76S3lV2hV+Xk7iOX04+YppvT9mfQk7Za9bW8PCw+ZFMokXCo" +
            "OXOPXH5VC0OmWupXMkbwyMDHJ5I69a9OCue0triz85L9TrJ6fKo7aC2icM0urSO8h5HGB2OCPn1rDWuyXiVw33MWoKQ+7z+R1pxh0" +
            "iM8WvX0bnYlnaNo2IVQSgA5GQp7de/tTFWS5lXDKTg7e35VJ4pvEgmUbxISec88D7YqRZzsVRVh08gn5fsfSlLJVIzQ2q7UOD2mIp" +
            "lEZKxL4RzkjIx2z2rXcm+85DgHA5P8AtVKW9tbW/nSA/M6jYqblPvHoMf3qSqMAliQUHAA/KqybNbMhBx7tiQWHYGZJRxg8j86QWb" +
            "ZNgoRLHAXHUE46dqyCwQaLLI0xKNhM8n64OSaO2lZbSMLXbIBbncAc8c4P5VZVJpnClIlTduOx2Y79cc89KTOsJkGcDBx83HTufWp" +
            "XB9Wh3I+rYlo1FRY4yBg5OfxqW0kYbRQlWACk5ORn0xW5U7e7IPu0afFjKktwkkJwOrHHX5U7V9TBSQXiSMeZ93jPf+9dd5FEM0X" +
            "yH7gYwQcH61FqdzqD2SaQk4j9ycDg5xwfWqoeukxW7d5bHjG13U4Pb7VkgIpQ7cxCgD13Y5z7VWSi+xi57Oy2yLmEhldc52nAB+F" +
            "Gx7EbxpEZN1Dg4A5+uO9U0t7qNrYqiSNuSeWxjpvH5Vj2pe8dsQ/dIcEr6DP4VJXOm5irRUtknPF+H5X8qSBo4TI2COFJPb5VJs3" +
            "T7Ptp4IJZYpCI3byQPGOvrjvVmK9aJCYt2Noxn5ePzp0cG+O6C0j5C5I7zrkHd37c5+vWmfYaZaTw20sgT59xJIDg4/HFZXRfXRw" +
            "bSUy7J3aY5GDj3A9M56VZ1DqV1FFdGUtJEx+YKB+XjGM+vSk0VRhS98k04CQx5xjGcH3rYpzlFS7HE/DGv3ENtFFM6KiyAubRiAQ" +
            "DgZ59h06VJ8OqV0ITIm9SD5jZHOOePxrRlvJpWmRW+UbAnbdjuOT6/SmSm+pSVV4pIwD84GQGx+nFbEzUvN8jWq9qtxpcURxpaQ+S" +
            "8imDMctu+o+X6Vc+2p0YjKhPcc+vntXauGEbYhVYlB6H+fWryi6dsTS6WypI9pW3jI8oEjPzDv6Vn1fSxqa3Sd0VLqb53KWKgk4" +
            "9M5x39KmdvZW+kbovcP3guMg7uMYPr2FeaWcl0zJzSsqktSx+ywMBSSAMnHr7VG7+JIopJxdunqR1+vNKo5J1o05yjNRd0VFCuFt0" +
            "gjcMjOOmT0p4s9qtbDOu7KPLj3xj+dSWuq24W0VtCFRAu/Jljwcg88joM1vpte07O8h1I3KvlgLkDJxg8D1+nWrL2ce0dS2d1Zy4" +
            "wsT7oBwcH2+9JuJ7x7aFpFKSkcc4+b64xWf0nlQ1rc3IZ5Ht8yRgY5P4Vea0d0YgSjEhhjyr9femL0VzKpCskXoQ+nJoY7b7M+G" +
            "bXStXlmXyHdyz3IOe2O1cMbi2TCKRuBzjIFNsa7aSIn3jIB5OMkZ+VQec7I4HqMMVQ+4fno0jZxcK2GkFrfUbOGZlYMEYx3zj1p" +
            "jW8dxdStIGUgR9w4H40gEVmznuXU3T7ZPp+VSNozwFGcHLehpewdz3WpNblyRSP3XIJwfpVZRFQ+0sP0GJ+XWMikMJdaEkgAbSeWf" +
            "Wpo0p1F3FDaXHwpxoP3gcdDn61NFRwcoIP45+1XwYOVcYx6VtfR03Vhlf5FfuoYz2HOB7Vn8q+Z0OiF4l2yAPTH86n0bTUkKf3O" +
            "Ix/WpJJkY9NuB/eqtNaRQNJOGBx3Bq5k2r5HjS9otmORfnwR74qYIDFj1I6fnS9utQds4B+9k5OPrWk4cRW2hXx2yd5PoO1NjfvL" +
            "qo7Zpms3R7S4hfIG5vujkHkVQ6jmaJRyUUnj5c+3+VEnAKH2kH6f41T0xw0YSRmBZhnnePp9qpV56TYeMf0OlbDgX3da3emO5g4" +
            "jJZC43ElQqD0I+nSl1HSIkVsxADkHrxzWL0jTXmsZJbOWQKOVLnn9qFuRbVY0Jw4P8AOpJIJPzpD1svFyO09zWtOUUbbdl7P0p3hR" +
            "bPaqESJhZkUknGfWlD3s1MYVwxjd0HfBrQDqqhVYHAA4xmmfvhrMOd4yDjPWprsknbEWtHmFug6k4HNNXd0gXGW2k6knFbFW5FV4c" +
            "Hoab5ztkLsAOuBg1jPzNr2ArWK23ssgwuM7WVt2PPOO9K2i5Ly5kVUZJydp7VbcUQlV3gYycZ56VJCqADkZpXk3Yi5JuaSTyY08A" +
            "HB6dqMW8ZxtBBPQepqrLwq7GVScc+vX60mknDxyAoHPNeaVcV2xPIJWWKpJycDr2pJpAA8yLhx75x1qWLUG3JR1z6U8r5P/AEO3Fd" +
            "w1Q1RIUdd3OePpQUsUrZYLZJJI5wQv8AStwbq+e6tJu2pNnZQiofl5dz7VQZB/lB2yOPT9KWM8hu6UA5Bzj8K8p2tyV4KSV2pHn" +
            "cfcZwf1rFyVxAh6wVxx3FPdN6lC+QoPu8f8aJtEkkhtrZoigbB5JYjrzj1zXG9d+0+RGk1sm+fTj5diwc5PWsGOJInTZ86iOcE89" +
            "B1pG1ZZuRnDk46H6VQVr5c9cOeAKIbuKGVzyef0qUqN2PWTWXg5kk0u5uYhbgFj8xbJxgHjP40z6y37yWikQgEcZxivJrWTkFXHt" +
            "85rXVwuV2BwyD6jNKdVJv60vkOmSxwsjmR3Kgnrz+VLq2mX8sojnD53BsH+8A9e9d1dQvCtI75ZTrwOKR7UuEjZQwwcDg9MVLZJ" +
            "vYzhwiua18LDrNu19NxK8Lb6UrwGBBHMvOD0z9KgGzJ4fT5Y5V7Ru6Bj1PpSae0eUIRkj8pBwB2w3vT9aXkpfcrZLcxDZOJWxL7" +
            "BcnvVWHRLe5RxkI7ZF5CgDPOeKn1HSYb6wt0wXuGViQVHlyRkHkH6VHSzuN+bGQDMjePdXrg8YqZSVYyjpJXK0KSjfw7EkYCjgcV" +
            "Vw9p2pWQkME72L+Pp6VqFJcXc8qxhaAYBwQD+fWpLWG1tvLMi25SzYHIx1x0qQOXHIH8aSmaUTQR8pIwBkHr9q2+PSebYSLkjZh" +
            "yA4wPp6Vn0V1PcwL5QyRKfkPXI4xWNE8iLkNkk7wM4x17U9Y7aGeKV8AQ1jC6b2JWFrdyM0MG5zkDA4+vWnpdXEV5HEI2SNud3" +
            "HoO1dQxhVM7cY4K49M0jWgLMSocHIz6kVPqUsvpY9oXtMI+UBx9+K9Wi0i7WGYn7rfIMbDg8evWji6hZpruNwAjDRqsY2uz4Jz0" +
            "zVhVJmUgeUFh3H4U+YhFPGZ2iKj7oB7t1qq13w7bEaKGy4yhDgD3AKR9h/lqsuSsLYrOOUPlI6+xrSHLtPOY0bjaeoHQVS6Ff+" +
            "opYyCx6ddhUS8jYwKDjg9uKuM2B7xj7pOM+ntVnLqE+l42snY8/eWHMfPr2rFv7+1QxCGXePZlU8n5etNFjJoJI8BtwOeM8H5VB" +
            "HiSykK9gRn9a5h3HyM2JYhD8oJx7+1dLoHiE2l4zK8xL/AGmAOfl6e9eUoq7j9uOvtV5NcViSPhQS5KlcE4OMDnr7VDpGmCU3MK" +
            "9xFo2OSRk5xn8KzlSO8f7mQeO9N3E5wHkdxVv6H7qYtMiHOHODzrW6pnJnmMeZt6cV7Qk0W7Y2TCSd2M8fN7VDSwQxlpGMuQF4P" +
            "njtVSj8g3G4g4x6Uzv7JSwxMqN5T3J4+vWnGVh0FKQrEvsgFlZiC2MfQ+tOvrho45J3b68H5VtMUS7m3gZ6H6U1Ywd5WuGHPLv" +
            "x61xVPM0V6eZ3YXBocbT9q8fPpQrozyQBnaw6c56Vb8VxB1DgArjB+nFTOSVDFCDnPTP5U+qk8bSGR1z6VMY7kDSCzE7SqkZwcZ" +
            "9QKau2EsAQQCqOg6j8a5+h0yd4g4Q4H4VLJr02CczxKOegOcD2pnU2yRs8BgTtIwF57nArXJw9lbYicE7g8DAx+tYdTcyluOzgAH" +
            "6H86QDrJTsSM9Mn9q3WcTgNIQMc7QCCenPrSgF2I3K5G3j2IPpWQ1vdOEjQhMZgM8c8Z/Kgn1a2hZZbVd8H81gQBv3bHc8etKT" +
            "Uu7fzQHX2rD1ywBhVAG0ZP5j86m+0Ww4J6fWuQcEXp5nO8L2pKXUbA8q9T9KZ9hUhskZz6V0UtQvdfh8ZiPcc+/lWbUDpOJIGzl" +
            "D8fTFSvLtyQtjPqp+VSVm5OCD0GT3r1CKZWdCSv3Y6Qcc+tDcrRsxJ79uAK+xqP3lZB0B6H5VNhXcPPGehHpQkaODsqkjtoQev1r" +
            "U7P9qP7kHcnjjP5VjFfgqX7Qx+4Dj3zQ5A3H3+H2qrxHAY4J9ucVnh0wdoHyew7e1PNpDcs7wucmcpzxUJVhL1C9nUO2O1MRiMV" +
            "6M+2M8fxqXW4fY3yKx5BDkHPOf0q1pUFuJIX+8PxqJs7e1KH0Jg4Pp2qXQoW3faojKpJAGOM5x7VpWI5gxdCxGxvujOPp6VQ6Z" +
            "oUb3MaqQ1wTgcjjA6VLa3FJBGiDKxYk5HOKg1L7FZvKUvy5YH8aNGpO6Ecd8ZG4H3r9H2uPsLIyK5x0wR1rP6G8UyHEjkHjFWZ" +
            "I7kgKoIGSOT9KZ9i5K+R1z7+lZ3TUp7YcNeABwwOMfP8AnV7pWiJMpYbJxg88f3qGTzHlE4yAcA4PrVxWjRmCg5PauZ4Yk1hhcY" +
            "WJwMAc89qWTxlVC9fl5fSlnL8hTb+BAO/FTuVQKx4PfIOMfrS2tf4QYrZ8CBksF5wcdD7GtmRKFJ2gDJ6j86m+1C7ilHYF89B0q" +
            "oWFvGJmcg/XipUicYzjJPpUu2ol6K9EaXxaS5Dtj3GfStLWY4nZuSGjYjGGI4J7H5VAdCUCQjlV3RwPzqO4nChdwyj5iCMcVkp8" +
            "uZJ2jIzn+lOo5adp2RLe+4cN1x6j+VWIYN5hwPQH5VLa4+RhvVQe3rU8l9GCWbyxEcaF0cEDp6U/PzRSVYHYByc9a7CMxSL2x1" +
            "3h4KTFHI31wGHjPpR5YAEk7FQflz6Vb1X2Yp5Y1fMz8vb6U2G6iAX5SOT1rfi0nNTZsKgiMxkM4HPOeKjtbYMNwc+w3p6VSM6Q" +
            "oHyk5HOc5p20Z5EE52MOYj8q2a/YDRG+mO3tlQuFv4pB3PPGe9MN1bPeRMOdjZ7e3SkZvvQo+chQex/wAqvIb3Q0OkUPcTuopCRk" +
            "5zjr3pBOB5QAq3YqZ2+XrjFSJCzHOa+uIU5S6EdpJHFLJsPcPjtVYwMq38M9etKSZiQQCRyDzU37Oc59uPzpyzrM97HM0DykJnMZ" +
            "j0PpVcyY6Nv6iTgce2K2YSzpZjMoAwdx3OfWltq4ZwduBznnvT/nSgP5dPnQyqOF2hiRBlCcY5Ge9WtWwJ2g48dK1nDCTtDEgH6f" +
            "xod8dQe5zWw8gqK2Fe7YzgdiD1qQWmSJI2I27T7e3X3qeUwKstpGc44yKjS5xjOQg6H6V0Z5chKaT0cbNlqcpE+YXG8EjqOPWl" +
            "u4XLsMzkjp7VqcZTvyFHuM8fhSOxEo5zgr3+lVLuHVnXtrQNaACZBT1B9qkldseYiSN2M4J7HNa15HiyD7qjv5Z/pSYxwYwSfc1l" +
            "VR0/K4Q7RHrW9cRQvpMv3l5H8Kmt07frLHgrnP5UjJv2mSQ4OPX0q0UW8GBwMcihjjpXlfnJdDUJbXs5+8x5YYJOfn7U7UrOEae" +
            "Qn5c8dqxvBcZDzZDK8nO08dKZpWn3EkjS7Yp5x7cV0UKW5XtqMOjxKSWTyZAjHLH8qfPGyblU445zxXKEgMEtuI9OaQTRthCjn6" +
            "VQjLrZ7Gji6bcqUbURWxI93JJ59KUUshQ7yckZz6VrOR34BU4wCpBJB5HsKd3JlbEsuc9elLrcv2lUHtrldoTaEc5wcVOU/aiONr" +
            "Zj1z7VmhJx0wc4zQ0b/eLgZ6HPPrXQOeSVm48J2h5WWx2uw9Oao4mV1lXTkZ71j2HB+tp6U+5lyxY8eg969bOUYpdTUuHIhiF6B" +
            "B1zVBAQzLjgjjvWY0lcgMhc/t7Clo3+8+SBioJHH+ULNLdM2k8hhHzg5B7Z6VtRbUVTKwtFrtIJnK4zwfzt9KTLBqcM0aIg5JHP" +
            "BrmbkxvcjIA4G3FB74b1ea+ciSsDxgw8bRiSMMNwBux2qpx23nk8yKpG7cDPB9cVn9XyOy2qJGQowc9qaxu47ihw3uo4Oc/Supj" +
            "LOvo0Lb1G7Z1d4ZUdwOTV4lmSQ8hcgcD616tjB3mqWJwD8j0qZ2yk7jgVyRC1aVIW37v6USdNspKpwi8/xq+0l5K4hKttAP2jH5" +
            "VJczqof7t3f2z2o8u7q0Y+TJtpgk5CgHjvVx0HSo7q5to0klYhwecc/rXqULgrCvbr0ppiq89AX64ql70WsNXBYyVwQWcjnPeh" +
            "r2xtpUjTd0G4Y4/yq7Exf3jYA9+9KX7xuhHqrDjP1NbdiSzIpXR2MNrNqEMjBkPXvVmq6fegBwfyqgYRrW6Yr1wPpR8rD1HIXS2" +
            "fcWHlKdjjHcVn1GTQW0YH7uCfT+dSUVMdsyONvGPQ0SqxkKk56jgVIXcsUWXDFFCc4zjjFUTZ33bSFc7hjj3rJ6bs2M4+V1A9SP" +
            "1q3aPMFg7lzn8qfYfZNy49Mf0qkdTO5Wck9B8qR2tyPTbxxXLvr2tPqEMF0jzB15wvr8qW0+4u5mVDkE9MQPzojtAE4wvoM4xS" +
            "JJFnJ7HIOa6Jy9z9ahje5zwhJtRjPMQdfWmWli7fZCHBzg+vNcu0gwwWbUY7jtx6U2WfY4UKcnP1yK7uDsb3OsFxGVOCfToKUyM" +
            "kSDjB71h1LQ42iZpRkZi3PlB/lRbrSWsJiN4sYwM8ZxXnnJdDLhqqM11bbBAXhcxmS2mAEnjI71hW3aeZT8x6efxqQTxVJXhLkt" +
            "j1/DUWjWUQlUBODzjvXqELgq2N3OD3L4iSJmQKu3bp71My4Vvm4xn8qzsjd5LF2Yjv609DkH5VO2KIzp3nMjwD/AOpS5XJZ45cJ" +
            "tkbM59x+dOZZC2OvTKinYIOVUegxQpVhO6LxqfrbMwuLTNgdd29KyKwafE+FJIHv9qQqKcYCk59OlPj3zc8PX3rqHKXRT2OCY7m" +
            "GRyPv8qktPtpfNRFCjk9Oe+K4VsHKjg7W7/AOg9aqC+lZkABGMgcU7u0aTwPS6G0eDDPBbSvbAfvJwufQ0v22PluOvfAq8xj2mK" +
            "BVCk+/PrioLW8WQqFfKHHA+tO2kk+DkiJzJJrCpWSMGGyOnPbJqOQMFtx39a2XLdqM9uyEZ6HNPmXLdlbGemcVcVUjQfKAQOgOSPr" +
            "jmmRDrsAIJwOo/CrujnFa6jqg2LlaGQ8YCmO7jFZXUtbHnWobAY2IdvI4qJtU0ooB8wGMAdaTXpZhDE01xBLPOiuXaME8Dk56UMh" +
            "8oT5WjJwOMZxWdYqBkY4B6etQlT/M6H+EH1Feec3cy4SXk1yPQ6JJNB50nR8Ag5+vWkfULeJ2Ubzk5+uK5fvJRy3IYEE/0qLj/S" +
            "kI+Un5vnqPSvVxOVWtpybOQkkW4Y8bngc5+tWqYsN1PtJJAz7fzrLtYJpmZcdixPy96u28TIwwo6Gr2UaDhRQj1Ov1PUI0wyEj" +
            "A9SaXsfy8QpHGAetZVf2g7sowA3NntV5riNY0VtgjjPX8q7lmk1E7pGxsb8nhyzF5CSEKOT16VUuLmR5bS4J3HxjHNZhXWJtY2c" +
            "7CR1HFSmzkJEPcDt9a8wk5KeRwGF48s9pNlpE7ySSwIYcYz+VKr3FgrbYOSeTUMCxJk64J7E8VXLJ5WZGOerDR2tLe67nI0mSSYY" +
            "gAfWnRXXTJLuYySQv3R3rNtyhG3IPbGe9Q3MthK2eMcVyTs47nyBf5LQ3Tdwqg7knnvRul4x9yjBzjmtOTk6j7xAwOM5xUJbKvO" +
            "elSzjhP2Ube9nHz9Ke8ZUduP19qMdjTj6nKpTyj1xxWXUiJJ+6A2OetNHWwYE7Qo456Vh44RKeuXAAp4s0MsAhQWOPxrL+1M7v" +
            "YpPGJzuR0p3imMv3YkHPOK58hyvj5eP1pC7j1Qfce9OE9hp5ivDqO8ilPP3cA57VMjsRqFG7gAfSqWyBSAfPdj9aSQ8ZshFz0Hc" +
            "c12TnLek8DG9xtG4OMe+KeL0uDFBDgbskY6Vl0vRHhht2Fyo9+9SxXWyVUSe38DXkkvI3VB98SvryV5ESDOv3S3JPXitqB50aRyR" +
            "sHIOTnnPapYEA47cEnp+lYGo2MklvLBKhh1zj3pTdw8qgA/nXpcJYWe23c4QeKj1DQmU20SAfKB3x9xXJ1IChPlQnBz1p1nAxzg" +
            "Z59qG9yNFO9iWPkA+xzVJ7WzzI44wOc8D6VlEV1DEQQEdd2aJk2DZ8wHbmmuEmUhj1JOTx0r5mzjvPSsVcpP5MaTSEprthCxHUZ" +
            "5+tR22pXqsMZXp+VZttrEWInXuTzntQpVjIQV6VRa2ZkU8yQAAOc+pFJ5hBcOkYSjYcZPYVz1OY1m+XgABsYwQRntRtFOAeCWJJb" +
            "xN1J4Yke1XtrmW3hhjYti6HnoB9u1Tm8d40akHlVwyMZByDW3Ls4uRWs5DksExZH3R19cVKIDcSc5HVup4xWabS2d09zIJjIyxA" +
            "7g4n2p05G+RT0PGPWvQITULQ0c3wlx4k7wc52+1Nxw2R8/gOcg04QK6OAASRnOKRZFLgsfXjg11Tm3aO3p9Lxcsi/e0doztznr" +
            "VZka8xYgnGMA1xgiG0ng+8CB0xVAAyZWg6dQ0R75tg7o5OSc9sCrEiBg2CCehAp0g+hSx2Qn3XIPT86aAIQcdD6VTtps54Y81JP" +
            "y9BmgFd6qHLYweBz6V6lGwW4aoIfO2/wC9Q2+Vg8c/0rmpYQIGM7iVPoKBcXq3ClvPhOKTM8xtxmWIseBjr9aXtHvUW3kuFVCSc" +
            "AcjJq1Oy9zgHHt1qF7VrZB3IA71ZnJ6y3LcXh4lyM+ZJaUHBOByOlPGcZeZp2IA5OPwq62O+PLQ2MnZg9qHuEijjlcA4I714q4y" +
            "oTpjqVuULJ5j7n3qWRQeQB1+9bYz2uL9mlRkKD0zkH+dMW4d93Fhhj+dZpxdzzErjnj9KPvHHRskYBx2ryV5dB0Uk4DyrMMDIIx" +
            "V2xvWQjnnyDlCM4+XHnWrRYEh32jYjPX1pNQ1SS/nhbMzj5MZH0FdmW3QqF3KUXqOJCZAy7dbjg8Y6UjZilSCwI+YbfpT5POOck" +
            "5BPJ9aXWVwTnHIFcdyZuNk+QkjqEY4+nSrJWkUkn2cE81luyNFcrn2hzk4qtcwSST3hkVd5f6s9oSYJDBtuQD8KcvmOClWyxGfQ" +
            "Vzm5RE2bkJ+vWjRWycY2V3jH5V7QitVvLQ3N9JY+WRWHIx9arxQWQm6kx2BA6H5V5D3E09+q7gKk8ViN8bVx/0qdtRrkATkfrT" +
            "L8iPd9aWMLlDyTmniLk+yjmck8g9a2AjnUc2/0vSVnhLSfmB8w56AY6VtWhDq0aPKwjoPzrl2vJlhx6fepbUGMg4yQOOetfOUJ3" +
            "ftS8+gyj3y4XzEafKzMOP5Vb+04tCbgZI4PpXKE4LONp5Hatcp5U0dV+FctySfWfS8jfmNbkOTnsM1qm2tcszvKhg5FeaS8E4j" +
            "wOnWuaQSxguGbb8h61Skq43MvqtoceQpIwePRiqqMfKdtuRjuM1h+Hrm3uC8kL95R6f1qFY1WHOD6V7Sx9pJXuh6M23lFoVjwMc" +
            "gZ96v+0ZpGJ3LwPyc4rjWzG6jC5+9yOc0isjrb9+K5ck1M6Hc6qwoZWzg84rD6jLJOJAqxHOBxVj9m3xKH2PuTXHlsJ4CfPrXq" +
            "Y6SL9zmh0+8h83B4IHU0Wm2bkLnOPSri7ka5jb5LZG7+f8AKkfU4h+Teq8kMT6R2sZYMOh6Vrt8LbWSSJoJT6DHFfIacZ5aCCN" +
            "QDuCRXUeH9NlubuxQsFYgcg15ay95KwWMXd61+vaInG3Q6OkhaMp4HE8jqAc49PpVLxBHbwSSIZHbH5HvXk1ivFKyMSgPocHmp9" +
            "StLjSLeXOBGEJJwcfKvMOTS01b4jj8cBbcOjLs4ByfSnIXWJCZoxjnvWN06uNb2Hz55b7mRkJj8q6rS9S0yzvp5pkZjzMfhx0q" +
            "bH4x0S0x+SFiS4Ac4rTu9XyPqp0F9e25iSR8gLS4x7Aeh/KouPHNoftfI4PYVzqlxH+8wBGQecdKB4tA2t2ZVkjJwOlHFctzHYc" +
            "j6qYZIWnPLT85G4kAV6dvrT7KzhBtlXzMZOSfamQeGbL7BYm2kAKj3rq4qjBHHE5UEceq98miuNNdMbfyl2VlsH+nU/WoLm99Wq" +
            "jbyQCCpJx2rkR4BFyo5x7ZpXs5iWQpkdBmvVjlGOuxuHOKGQwWbBYkzjAAH5H60tobTujrsTLbT7uM85/KsC0C2l0wWWV84+Xg" +
            "fWkF8eVIAiUdQOh96yeX3Mw5Sk8WxjKge9PV9TsJY4g0sa7WDnIo+YK8qjf03VIR/XP3fvP6Uh1y+uLUsx+T8xPB4JrzSSXVX9" +
            "Hxtf9LaQaLqisVZJg5BzWnONovJyp9hXOpI4xLp+HpVR4mAJ8/eauVy+btqHVo5GaRk5z+VS2lvDPYCSJuZtpH+FWX0aWNsxCRu" +
            "PHNVp3BZFEdeee34VXMcQPg5bmNtcIjyOL0GCxFFnHuavLbgoA6fKvLJnxyR7VlOpKrHG7t+ROuPei6xC+IpG5yD9KrF2KxJSRk" +
            "VyXcQMBrbgjs/OoVvlfjC4PT6Vh/TOulzJfCLQiSbN3gr9CK7iO2m0uxvxJDd22wf/ACqzTcR5bjcM1zhVm69u38qXTLQZ7m4m" +
            "mV17lMjAHOWHOaKqluZO6ITMk4x+daHh3Q5LnUpbq3ZPMMq38a5IayYXb+hoOlWsVzExlyQeR1FchLZprU5IpEXVlO0Y7flV6Hxo" +
            "tfRKXsMdTnA/Ie9eLJ+aTWCxldL7RckWT/AN27U9r7QrfHFfYy5XqfYVxw3o0NuRI5UbJD0rE0uHVpvMqCA5AGcE15pK5PdHXL" +
            "ItR2mrJcwmQ3LsWJH0qlrOlRT3K/vRkV6dx+VWaCYKMbfPrW/wCGtLjpAkmOWMq9R71pJ3v2XSyvxhYD6r4cVeUxIUI/qwfSm3w" +
            "svU/IcEYPPT6VT8UMpK5HjvXcRqIq7HlWpo63Fbt2JOQevGaeMA3PhwOtMtrgNoa6s/X9GFb07XFD4WhbzrhZG5AOcoep5pcuovL" +
            "ywsQzFjKc5qNmlG0EEjOfcVK1v8Ar4Lt/FIAUAH2qUtKXNk3iVhtbyLTVXy9ygKcda8qrPLyscKxlVPoBV6XQILp2Rd7k5qXVLK" +
            "3kWOaVSQFnpXyMnN58TUS78EfYbriKoVGWBI4zTHsVUjrGD14rw6NUGEPcH1FX5bePc48vp9quM8TzLk/LgnjuDSa1YGU8jcfePA" +
            "qx7guM+eQHGMVk2BGSIHg/WsKa3JqKsbbN9/U4SvkyyHmb8OKWK6dEt1HlgT1zWrJqEdziQPZR6cVKbLTniDt0P5V1zKceZtHPN" +
            "cmGmyQtbqvqGMP8AGsfpN1GZ7aPliJC+Dnj8K8oWOFiOTgcZHpVfVNK1E7FDTytvBGeeornShyWiFbUfVyWLwTo8GravHsDfUGa" +
            "9XiBKjANsvOQOp4pdyLWyWs0HA+fBx1H9a6pQXxNrbHrJo0whnDL91WQ8VGY4ZbQzBOGGcfpXH9WuodFEVoQON3r+Nba+52w5PK" +
            "JnnP8AKvJIhMsSf6r7VLcSGeTjAd8VeMO5qH7M8pIxxjrRpbY/XH1p9xEdVZgR2wx+FULhSmR80qcD3rp4mcW+TuixbVJCIY43k" +
            "9T70bi1mZkRQOD2rC0qJQ52rk1r6i8f/P5/rSLPTZzppFEWL7v5iljG7gnoT61vxzBXKdMErD3UKorES6Sx9vc9azfSrfWDXfw6" +
            "zRLHOw9lxcgexH+r1q+5ltY9uN7MQWjYAGMf7+9Z0MtyId1X8DWXRl7y2wQdj+tWE2wBOQcD38q7OcE9DAyh01xyuP8AKppnDBi" +
            "Bj8+e9ZVQxzXBxwdpPuK0nDnqvqD6U9pHYS2MZ2T6liSDqu/d+9e2kPHclObnKjM3hVdkRBwQSpxXKZ+EjcMuNlWD1rpPG+s3CT" +
            "y0iYIp3G9yFgO9Vg5vrqKFyUyFK85Fc0pWSMmCz/rWv8R6SC6ujJY/I8v+FQ8LxJbvlfJPmGmfkcMcVHO9+Ms7ZGK2QHWuhoRc" +
            "pFXdDHPpSbgANx46/jSllaTJt5XVpKGEjHC8BTwW5CrnqTiuCbUPcnsaa78NrjPkEMQTn3xVwhTwjVVB66f6UUF5bvygo9iK5rZ" +
            "IrYzd0HE1L5WG7eMqO/4VDZiTPoh0yWMTq4QZ8YIXHao30djNG+3I9Qw9PxrTRQfruZCBpn2szROVBgBR7/AMq2XOMzkZ7E8jWd" +
            "TAlhx7c/6Vn3NrFdxH3jntioStVqOZPhHbB1xT7Ba69QBL5VVOT96Vtn7rLx5jmBJqOwk5U4ODVXU7zUIbzuj5QTn9aKc3jp8dJ" +
            "byTlc8gNS+NhGVuZEJA4Gcf1rAaM51UO7VGI4bkcE56VX0m4lzVI/M/wDYLGqnauM2w7sUmI/wAH3q5/YdtaLHKY5A7+pGatol" +
            "g2xqN5c7VA4A6fKo81oizZC3Xv0pvTyJcRXBJA29PB+pNedrLrhb4fVA4v0tzFJvXy2f/ADpyhMVMvtJJzj8q2Ppcl3d3K6JLM" +
            "VJjvt1FdJPHdR84POMVrXPVvQytFWyW5FJa/h5/1rulFFSo3kaSpHlbXgW4mG99tbhT0xkVwSLOyMZ4BqSNo3rEQTgkkjvVAtj" +
            "PnwzkdxmtNN7nupflil81P3sZGc49j+tfMUY0l8j0sHGfZmiMUoc/6VygLi4IXyzeU/wABj+pNC8vNaHMXYD0r2MxSwtpnc5qV2" +
            "NspYrkE0iKSUi6ShygGvpXITQOSpAPXtTIIguZjkdKqKwDzMqDng10zlzm72Ol/JRUet+neKgKgdyKsCefTjFZ1l54W4Hynp6D0" +
            "pVURVjGXOTXcTFqnL3+ZDFAf3u/3FubovQxXTqVskZH0rD0kuSpwxYevFPuILWwcjnv9a8tm52WCWYLVGlTmtYCBtyzjPoK51LxW" +
            "mCc+Zs561xGgmD5XVxs6+16+NtRkuzRpSIf3XbgH8qY0c6tvlGSCo+pry6yhtyduG5rqfDmqm40xWx+VmOSvc1NayjOw56DNjLk" +
            "9r3ttk8RrhGfkssrjH5UMa3M8yM+4rj35V7aTKHEvvwc8HOK5w3GRuwKYsu4cy4PpXOaNuNtuALsOuKxo6W5VOR1yOfStmbP6P" +
            "SaQ+lV4uijhQ/KjcMdK//9k";

        [MenuItem("Learning with Journey/Repair Match Card Backs V5 Embedded")]
        public static void Apply()
        {
            ApplyInternal(true);
        }

        public static void ApplySilently()
        {
            ApplyInternal(false);
        }

        static void ApplyInternal(bool showDialog)
        {
            if (!File.Exists(ScenePath))
            {
                if (showDialog)
                    EditorUtility.DisplayDialog("Learning with Journey", "AlphabetMatchWorld.unity was not found. Build Alphabet Match World V2 first.", "OK");
                return;
            }

            try
            {
                Directory.CreateDirectory(GeneratedFolder);

                byte[] bytes = Convert.FromBase64String(CardBackBase64);
                if (bytes.Length < 1000 || bytes[0] != 0xFF || bytes[1] != 0xD8)
                    throw new Exception("Embedded card-back data did not decode as a valid JPEG.");

                File.WriteAllBytes(CardBackPath, bytes);
            }
            catch (Exception ex)
            {
                if (showDialog)
                    EditorUtility.DisplayDialog("Learning with Journey", "Could not rebuild the embedded matching-card artwork.\n\n" + ex.Message, "OK");
                return;
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            AssetDatabase.ImportAsset(CardBackPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);

            var importer = AssetImporter.GetAtPath(CardBackPath) as TextureImporter;
            if (importer == null)
            {
                if (showDialog)
                    EditorUtility.DisplayDialog("Learning with Journey", "Unity rebuilt the card-back file but did not create a TextureImporter.", "OK");
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 512;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.SaveAndReimport();

            AssetDatabase.ImportAsset(CardBackPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);

            Sprite cardBackSprite = AssetDatabase.LoadAssetAtPath<Sprite>(CardBackPath);
            if (cardBackSprite == null)
            {
                foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(CardBackPath))
                {
                    if (asset is Sprite sprite)
                    {
                        cardBackSprite = sprite;
                        break;
                    }
                }
            }

            if (cardBackSprite == null)
            {
                if (showDialog)
                    EditorUtility.DisplayDialog(
                        "Learning with Journey",
                        "The embedded artwork was rebuilt as a valid JPEG, but Unity still did not expose a Sprite sub-asset. Check the Console for the generated V5 file only.",
                        "OK");
                return;
            }

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            int updated = 0;

            for (int i = 1; i <= 8; i++)
            {
                var card = GameObject.Find("MatchCard" + i);
                if (card == null) continue;

                var back = card.transform.Find("Back");
                if (back == null) continue;

                SetChildActive(back, "Question", false);
                SetChildActive(back, "MatchLabel", false);
                SetChildActive(back, "Gloss", false);

                var oldLogo = back.Find(LogoObjectName);
                if (oldLogo != null)
                    UnityEngine.Object.DestroyImmediate(oldLogo.gameObject);

                var logoGo = new GameObject(LogoObjectName, typeof(RectTransform), typeof(Image));
                logoGo.transform.SetParent(back, false);

                var rect = (RectTransform)logoGo.transform;
                rect.anchorMin = new Vector2(.025f, .025f);
                rect.anchorMax = new Vector2(.975f, .975f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                rect.localScale = Vector3.one;
                rect.localRotation = Quaternion.identity;

                var image = logoGo.GetComponent<Image>();
                image.sprite = cardBackSprite;
                image.preserveAspect = false;
                image.color = Color.white;
                image.raycastTarget = false;

                logoGo.transform.SetAsLastSibling();
                updated++;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (showDialog)
            {
                EditorUtility.DisplayDialog(
                    "Learning with Journey",
                    "Fixed. " + updated + " matching cards now use the approved Learning with Journey logo card back. V5 rebuilt the image from embedded verified bytes, so it no longer depends on the broken GitHub image file.",
                    "OK");
            }
        }

        static void SetChildActive(Transform parent, string childName, bool value)
        {
            var child = parent.Find(childName);
            if (child != null) child.gameObject.SetActive(value);
        }
    }
}
#endif
