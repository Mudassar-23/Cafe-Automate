# ── Frontend — nginx static server ──────────────────────────────
FROM nginx:1.27-alpine

# Remove the default nginx page
RUN rm -rf /usr/share/nginx/html/*

# Copy static frontend assets
COPY index.html \
     login.html \
     dashboard-user.html \
     dashboard-cafe-admin.html \
     dashboard-website-admin.html \
     hero-cafe.jpg \
     /usr/share/nginx/html/

COPY css/ /usr/share/nginx/html/css/
COPY js/  /usr/share/nginx/html/js/

# Nginx config (proxies /api and /hubs to backend)
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
